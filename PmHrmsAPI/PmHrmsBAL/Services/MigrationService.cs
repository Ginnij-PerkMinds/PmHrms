using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly PmHrmsContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MigrationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBulkEmployeeService _bulkEmployeeService;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public MigrationService(
            PmHrmsContext context,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MigrationService> logger,
            IConfiguration configuration,
            IBulkEmployeeService bulkEmployeeService)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
            _bulkEmployeeService = bulkEmployeeService;
        }

        // ====================================================================================
        // 1. CONFIGURATION & JOBS
        // ====================================================================================

        public async Task<List<SystemField>> GetConfigsByEntityAsync(string entityType)
        {
            var normalizedEntityType = NormalizeEntityType(entityType);
            _logger.LogInformation("[Config] Fetching system fields | EntityType: {EntityType}", normalizedEntityType);

            var sw = Stopwatch.StartNew();
            var fields = await _context.MigrationConfigs
                .Where(x => x.EntityType == normalizedEntityType && x.IsActive == true)
                .Select(f => new SystemField
                {
                    Key = f.FieldKey,
                    Label = f.Label,
                    Description = f.FieldKey,
                    Required = f.IsRequired,
                    Category = f.Category,
                    ValidationType = f.ValidationType,
                    Keywords = f.Keywords
                })
                .ToListAsync();

            sw.Stop();
            _logger.LogInformation("[Config] Fetched {Count} fields | EntityType: {EntityType} | Duration: {Ms}ms",
                fields.Count, normalizedEntityType, sw.ElapsedMilliseconds);

            return fields;
        }

        public async Task<MigrationJob?> GetActiveJobAsync(int orgId, string entityType)
        {
            var normalizedEntityType = NormalizeEntityType(entityType);
            _logger.LogInformation("[Job] Checking active job | OrgId: {OrgId} | EntityType: {EntityType}", orgId, normalizedEntityType);

            var activeStatuses = new[] { "QUEUED", "SCANNING", "PREPARING_MASTERS", "VALIDATING", "IMPORTING" };

            var activeJob = await _context.MigrationJobs
                .Where(j => j.OrgId == orgId
                    && j.EntityType == normalizedEntityType
                    && activeStatuses.Contains(j.Status))
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync();

            if (activeJob == null)
            {
                _logger.LogInformation("[Job] No active job found | OrgId: {OrgId} | EntityType: {EntityType}", orgId, normalizedEntityType);
                return null;
            }

            _logger.LogInformation("[Job] Active job found | JobId: {JobId} | Status: {Status} | LastHeartbeat: {Heartbeat}",
                activeJob.Id, activeJob.Status, activeJob.LastHeartbeat);

            if (activeJob.LastHeartbeat.HasValue)
            {
                var elapsed = DateTime.Now - activeJob.LastHeartbeat.Value;
                var heartbeatThreshold = TimeSpan.FromMinutes(5);

                _logger.LogDebug("[Job] Heartbeat check | JobId: {JobId} | Elapsed: {Elapsed:F1}s | Threshold: {Threshold}s",
                    activeJob.Id, elapsed.TotalSeconds, heartbeatThreshold.TotalSeconds);

                if (elapsed > heartbeatThreshold)
                {
                    _logger.LogWarning("[Job] ZOMBIE JOB detected | JobId: {JobId} | LastHeartbeat: {Heartbeat} | Elapsed: {Elapsed:F1}min. Marking FAILED.",
                        activeJob.Id, activeJob.LastHeartbeat, elapsed.TotalMinutes);

                    activeJob.Status = "FAILED";
                    activeJob.ErrorLog = "Job terminated due to inactive heartbeat";
                    await _context.SaveChangesAsync();
                    return null;
                }
            }

            return activeJob;
        }

        // ====================================================================================
        // 2. MAPPING & VALIDATION
        // ====================================================================================

        public Dictionary<string, object> AutoMapWithConfidence(
            List<string> excelColumns,
            List<SystemField> systemFields,
            List<Dictionary<string, object>> sampleData)
        {
            _logger.LogInformation("[AutoMap] Starting | ExcelColumns: {ColCount} | SystemFields: {FieldCount} | SampleRows: {SampleCount}",
                excelColumns?.Count ?? 0, systemFields?.Count ?? 0, sampleData?.Count ?? 0);

            var sw = Stopwatch.StartNew();
            var result = new Dictionary<string, object>();

            foreach (var field in systemFields)
            {
                string? bestMatchCol = null;
                double highestScore = 0;
                string matchSource = "";

                foreach (var col in excelColumns)
                {
                    double headerScore = GetSimilarity(col, field.Label) * 0.6;

                    var colValues = sampleData?
                        .Select(row => row.ContainsKey(col) ? row[col]?.ToString() ?? string.Empty : string.Empty)
                        .ToList() ?? new List<string>();

                    double contentScore = AnalyzeContent(colValues, field) * 0.4;
                    double totalScore = headerScore + contentScore;

                    if (totalScore > highestScore && totalScore > 0.5)
                    {
                        highestScore = totalScore;
                        bestMatchCol = col;
                        matchSource = headerScore > contentScore ? "header" : "content";
                        if (headerScore > 0 && contentScore > 0) matchSource = "header+content";
                    }
                }

                result[field.Key] = new
                {
                    column = bestMatchCol,
                    confidence = Math.Round(highestScore, 2),
                    source = matchSource
                };

                _logger.LogDebug("[AutoMap] '{FieldKey}' → '{Col}' | Confidence: {Score} | Source: {Source}",
                    field.Key, bestMatchCol ?? "NO MATCH", Math.Round(highestScore, 2), matchSource);
            }

            sw.Stop();
            _logger.LogInformation("[AutoMap] Complete | {FieldCount} fields mapped | Duration: {Ms}ms",
                result.Count, sw.ElapsedMilliseconds);

            return result;
        }

        public async Task<object> ValidateFullFileAsync(ImportRequestModel request, int orgId, PmHrmsContext? db = null)
        {
            var normalizedEntityType = NormalizeEntityType(request.EntityType);
            _logger.LogInformation("[Validate] Starting | OrgId: {OrgId} | EntityType: {EntityType} | Rows: {RowCount}",
                orgId, normalizedEntityType, request.Rows.Count);

            var context = db ?? _context;
            var sw = Stopwatch.StartNew();

            var result = normalizedEntityType switch
            {
                "Department" => await ValidateDepartmentFileAsync(request, orgId, context),
                "Designation" => await ValidateDesignationFileAsync(request, orgId, context),
                "OrgRole" => await ValidateRoleFileAsync(request, orgId, context),
                _ => await ValidateEmployeeFileAsync(request, orgId, context),
            };

            sw.Stop();
            _logger.LogInformation("[Validate] Done | EntityType: {EntityType} | Duration: {Ms}ms",
                normalizedEntityType, sw.ElapsedMilliseconds);

            return result;
        }

        public async Task<object> ValidateEmployeeFileAsync(ImportRequestModel request, int orgId, PmHrmsContext? db = null)
        {
            _logger.LogInformation("[Validate:Employee] Starting | OrgId: {OrgId} | Rows: {RowCount}", orgId, request.Rows.Count);

            var context = db ?? _context;
            var sw = Stopwatch.StartNew();
            var errors = new List<object>();
            var warnings = new List<object>();
            var duplicates = new Dictionary<string, List<int>>();
            var emailMap = new Dictionary<string, List<int>>();
            var codeMap = new Dictionary<string, List<int>>();

            var existingEmails = await context.Employees
                .AsNoTracking()
                .Where(e => e.OrganizationId == orgId)
                .Select(e => e.OfficialEmail.ToLower())
                .Where(e => e != null)
                .Select(e => e!)
                .ToListAsync();

            var existingCodes = await context.Employees
                .Where(e => e.OrganizationId == orgId && !string.IsNullOrEmpty(e.EmployeeCode))
                .Select(e => e.EmployeeCode!.ToLower())
                .ToListAsync();

            _logger.LogDebug("[Validate:Employee] DB snapshot | ExistingEmails: {E} | ExistingCodes: {C}",
                existingEmails.Count, existingCodes.Count);

            int rowNum = 2;
            foreach (var row in request.Rows)
            {
                var email = GetSafeValue(row, request.Mapping, "official_email");
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add(new { row = rowNum, field = "Email", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Employee] Row {Row}: Email missing", rowNum);
                }
                else
                {
                    email = email.ToLower().Trim();
                    if (!IsValidEmail(email))
                    {
                        errors.Add(new { row = rowNum, field = "Email", message = "Invalid email format", value = email });
                        _logger.LogDebug("[Validate:Employee] Row {Row}: Invalid email → '{Email}'", rowNum, email);
                    }
                    if (existingEmails.Contains(email))
                    {
                        errors.Add(new { row = rowNum, field = "Email", message = "Email already exists in system", value = email });
                        _logger.LogDebug("[Validate:Employee] Row {Row}: Email already in DB → '{Email}'", rowNum, email);
                    }
                    if (!emailMap.ContainsKey(email)) emailMap[email] = new List<int>();
                    emailMap[email].Add(rowNum);
                }

                var code = GetSafeValue(row, request.Mapping, "employee_code");
                if (!string.IsNullOrWhiteSpace(code))
                {
                    code = code.ToLower().Trim();
                    if (existingCodes.Contains(code))
                    {
                        errors.Add(new { row = rowNum, field = "Employee Code", message = "Code already exists in system", value = code });
                        _logger.LogDebug("[Validate:Employee] Row {Row}: Code already in DB → '{Code}'", rowNum, code);
                    }
                    if (!codeMap.ContainsKey(code)) codeMap[code] = new List<int>();
                    codeMap[code].Add(rowNum);
                }

                var firstName = GetSafeValue(row, request.Mapping, "first_name");
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    errors.Add(new { row = rowNum, field = "First Name", message = "Required field is empty", value = "" });                                      
                    _logger.LogDebug("[Validate:Employee] Row {Row}: First Name missing", rowNum);
                }

                var departmentName = GetSafeValue(row, request.Mapping, "department_name");
                if (string.IsNullOrWhiteSpace(departmentName))
                {
                    errors.Add(new { row = rowNum, field = "Department", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Employee] Row {Row}: Department missing", rowNum);
                }

                var designationName = GetSafeValue(row, request.Mapping, "designation_name");
                if (string.IsNullOrWhiteSpace(designationName))
                {
                    errors.Add(new { row = rowNum, field = "Designation", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Employee] Row {Row}: Designation missing", rowNum);
                }

                var phone = GetSafeValue(row, request.Mapping, "phone_number");
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
                    if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
                    {
                        warnings.Add(new { row = rowNum, field = "Phone", message = "Phone should be 10-15 digits", value = phone });
                        _logger.LogDebug("[Validate:Employee] Row {Row}: Phone warning → '{Phone}' ({Len} digits)", rowNum, phone, digitsOnly.Length);
                    }
                }

                rowNum++;
            }

            foreach (var kvp in emailMap.Where(x => x.Value.Count > 1))
            {
                duplicates[$"Email: {kvp.Key}"] = kvp.Value;
                _logger.LogWarning("[Validate:Employee] Duplicate email in file → '{Email}' at rows: {Rows}", kvp.Key, string.Join(", ", kvp.Value));
            }
            foreach (var kvp in codeMap.Where(x => x.Value.Count > 1))
            {
                duplicates[$"Code: {kvp.Key}"] = kvp.Value;
                _logger.LogWarning("[Validate:Employee] Duplicate code in file → '{Code}' at rows: {Rows}", kvp.Key, string.Join(", ", kvp.Value));
            }

            sw.Stop();
            _logger.LogInformation("[Validate:Employee] Done | Errors: {E} | Warnings: {W} | Duplicates: {D} | Duration: {Ms}ms",
                errors.Count, warnings.Count, duplicates.Count, sw.ElapsedMilliseconds);

            return new
            {
                isValid = errors.Count == 0 && duplicates.Count == 0,
                errors = errors.Count,
                warnings = warnings.Count,
                duplicates = duplicates.Count,
                canProceed = errors.Count == 0 && duplicates.Count == 0,
                errorDetails = errors,
                warningDetails = warnings,
                duplicateDetails = duplicates
            };
        }

        private async Task<object> ValidateDepartmentFileAsync(ImportRequestModel request, int orgId, PmHrmsContext context)
        {
            _logger.LogInformation("[Validate:Department] Starting | OrgId: {OrgId} | Rows: {RowCount}", orgId, request.Rows.Count);

            var errors = new List<object>();
            var nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var existingSet = new HashSet<string>(
                await context.Departments
                    .Where(d => d.OrganizationId == orgId && d.IsActive && d.DepartmentNameNormalized != null)
                    .Select(d => d.DepartmentNameNormalized!)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Validate:Department] Existing in DB: {Count}", existingSet.Count);

            int rowNum = 2;
            foreach (var row in request.Rows)
            {
                var name = GetSafeValue(row, request.Mapping, "department_name")?.Trim();
                var normalized = name?.ToLower();

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add(new { row = rowNum, field = "Department Name", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Department] Row {Row}: Name missing", rowNum);
                }
                else if (existingSet.Contains(normalized!))
                {
                    errors.Add(new { row = rowNum, field = "Department Name", message = "Already exists for this organisation", value = name });
                    _logger.LogDebug("[Validate:Department] Row {Row}: '{Name}' already in DB", rowNum, name);
                }
                else if (!nameSet.Add(normalized!))
                {
                    errors.Add(new { row = rowNum, field = "Department Name", message = "Duplicate in uploaded file", value = name });
                    _logger.LogDebug("[Validate:Department] Row {Row}: '{Name}' is a duplicate in file", rowNum, name);
                }

                rowNum++;
            }

            _logger.LogInformation("[Validate:Department] Done | Errors: {E}", errors.Count);
            return BuildValidationResponse(errors);
        }

        private async Task<object> ValidateDesignationFileAsync(ImportRequestModel request, int orgId, PmHrmsContext context)
        {
            _logger.LogInformation("[Validate:Designation] Starting | OrgId: {OrgId} | Rows: {RowCount}", orgId, request.Rows.Count);

            var errors = new List<object>();
            var dupeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var deptLookup = await context.Departments
                .Where(d => d.OrganizationId == orgId && d.IsActive && d.DepartmentNameNormalized != null)
                .ToDictionaryAsync(
                    d => d.DepartmentNameNormalized!,
                    d => d.DepartmentId,
                    StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Validate:Designation] Depts loaded: {Count}", deptLookup.Count);

            var existingDesigs = new HashSet<string>(
                await context.Designations
                    .Where(d => d.IsActive == true && d.DepartmentId != null && d.DesignationName != null)
                    .Select(d => d.DepartmentId + "|" + d.DesignationName!.ToLower().Trim())
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Validate:Designation] Existing designations in DB: {Count}", existingDesigs.Count);

            int rowNum = 2;
            foreach (var row in request.Rows)
            {
                var title = GetSafeValue(row, request.Mapping, "designation_name")?.Trim();
                var deptName = GetSafeValue(row, request.Mapping, "department_name")?.Trim();

                if (string.IsNullOrEmpty(title))
                {
                    errors.Add(new { row = rowNum, field = "Designation Title", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Designation] Row {Row}: Title missing", rowNum);
                    rowNum++; continue;
                }

                if (string.IsNullOrEmpty(deptName))
                {
                    errors.Add(new { row = rowNum, field = "Department", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:Designation] Row {Row}: Department missing for '{Title}'", rowNum, title);
                    rowNum++; continue;
                }

                int? deptId = null;
                if (deptLookup.TryGetValue(deptName.ToLower(), out var id))
                    deptId = id;
                else
                {
                    errors.Add(new { row = rowNum, field = "Department", message = $"'{deptName}' does not exist. Create it first.", value = deptName });
                    _logger.LogWarning("[Validate:Designation] Row {Row}: Dept '{Dept}' not found | OrgId: {OrgId}", rowNum, deptName, orgId);
                }

                var dupeKey = $"{deptId}|{title.ToLower()}";
                if (existingDesigs.Contains(dupeKey))
                {
                    errors.Add(new { row = rowNum, field = "Designation Title", message = "Already exists in this department", value = title });
                    _logger.LogDebug("[Validate:Designation] Row {Row}: '{Title}' already exists in DeptId {DeptId}", rowNum, title, deptId);
                }
                else if (!dupeSet.Add(dupeKey))
                {
                    errors.Add(new { row = rowNum, field = "Designation Title", message = "Duplicate in uploaded file (same title + department)", value = title });
                    _logger.LogDebug("[Validate:Designation] Row {Row}: '{Title}' is duplicate in file (dept: {Dept})", rowNum, title, deptName);
                }

                rowNum++;
            }

            _logger.LogInformation("[Validate:Designation] Done | Errors: {E}", errors.Count);
            return BuildValidationResponse(errors);
        }

        private async Task<object> ValidateRoleFileAsync(ImportRequestModel request, int orgId, PmHrmsContext context)
        {
            _logger.LogInformation("[Validate:OrgRole] Starting | OrgId: {OrgId} | Rows: {RowCount}", orgId, request.Rows.Count);

            var errors = new List<object>();
            var nameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var existingSet = new HashSet<string>(
                await context.OrgRoles
                    .Where(r => r.OrgId == orgId && r.Name != null)
                    .Select(r => r.Name!.ToLower().Trim())
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Validate:OrgRole] Existing roles in DB: {Count}", existingSet.Count);

            int rowNum = 2;
            foreach (var row in request.Rows)
            {
                var name = GetSafeValue(row, request.Mapping, "role_name")?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add(new { row = rowNum, field = "Role Name", message = "Required field is empty", value = "" });
                    _logger.LogDebug("[Validate:OrgRole] Row {Row}: Name missing", rowNum);
                }
                else if (existingSet.Contains(name.ToLower()))
                {
                    errors.Add(new { row = rowNum, field = "Role Name", message = "Already exists for this organisation", value = name });
                    _logger.LogDebug("[Validate:OrgRole] Row {Row}: '{Name}' already in DB", rowNum, name);
                }
                else if (!nameSet.Add(name.ToLower()))
                {
                    errors.Add(new { row = rowNum, field = "Role Name", message = "Duplicate in uploaded file", value = name });
                    _logger.LogDebug("[Validate:OrgRole] Row {Row}: '{Name}' is duplicate in file", rowNum, name);
                }

                rowNum++;
            }

            _logger.LogInformation("[Validate:OrgRole] Done | Errors: {E}", errors.Count);
            return BuildValidationResponse(errors);
        }

        private static object BuildValidationResponse(List<object> errors) => new
        {
            errors = errors.Count,
            warnings = 0,
            duplicates = 0,
            canProceed = errors.Count == 0,
            isValid = errors.Count == 0,
            errorDetails = errors
        };

        // ====================================================================================
        // 3. CORE PROCESS
        // ====================================================================================

        public async Task<Guid> CreateJobAsync(int orgId, int userId, string entityType, int totalRecords, string? fileName)
        {
            var normalizedEntityType = NormalizeEntityType(entityType);
            _logger.LogInformation("[Job:Create] Creating job | OrgId: {OrgId} | UserId: {UserId} | EntityType: {EntityType} | TotalRecords: {Total} | File: {File}",
                orgId, userId, normalizedEntityType, totalRecords, fileName ?? "N/A");

            var job = new MigrationJob
            {
                Id = Guid.NewGuid(),
                OrgId = orgId,
                RequestedByUserId = userId,
                EntityType = normalizedEntityType,
                TotalRecords = totalRecords,
                Status = "QUEUED",
                CreatedAt = DateTime.Now,
                FileName = fileName,
                SharedPassword = GenerateStrongPassword(),
                LastHeartbeat = DateTime.Now
            };

            _context.MigrationJobs.Add(job);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[Job:Create] Created | JobId: {JobId}", job.Id);
            return job.Id;
        }

        public async Task ProcessImportAsync(Guid jobId, ImportRequestModel request, int orgId)
        {
            _logger.LogInformation("[Job:Route] Processing import | JobId: {JobId} | OrgId: {OrgId} | EntityType: {EntityType}",
                jobId, orgId, request.EntityType);

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PmHrmsContext>();

            var job = await db.MigrationJobs.FindAsync(jobId);
            if (job == null)
            {
                _logger.LogError("[Job:Route] Job NOT FOUND in DB | JobId: {JobId}", jobId);
                return;
            }

            var normalizedEntityType = NormalizeEntityType(request.EntityType);
            request.EntityType = normalizedEntityType;

            _logger.LogInformation("[Job:Route] Routing to handler | JobId: {JobId} | EntityType: {EntityType}", jobId, normalizedEntityType);

            switch (normalizedEntityType)
            {
                case "Department":
                    await ProcessDepartmentImportAsync(db, job, request, orgId);
                    break;
                case "Designation":
                    await ProcessDesignationImportAsync(db, job, request, orgId);
                    break;
                case "OrgRole":
                    await ProcessRoleImportAsync(db, job, request, orgId);
                    break;
                default:
                    await ProcessEmployeeImportAsync(jobId, request, orgId);
                    break;
            }

            _logger.LogInformation("[Job:Route] Handler finished | JobId: {JobId}", jobId);
        }

        // ====================================================================================
        // 4. ENTITY-SPECIFIC IMPORT PROCESSORS
        // ====================================================================================

        private async Task ProcessDepartmentImportAsync(PmHrmsContext db, MigrationJob job, ImportRequestModel request, int orgId)
        {
            _logger.LogInformation("[Import:Department] Starting | JobId: {JobId} | OrgId: {OrgId} | TotalRows: {Total}",
                job.Id, orgId, request.Rows.Count);

            var sw = Stopwatch.StartNew();
            var failedRecords = new List<object>();
            int batchSize = _configuration.GetValue<int>("Migration:BatchSize", 50);

            _logger.LogDebug("[Import:Department] BatchSize: {BatchSize}", batchSize);

            var existingNames = new HashSet<string>(
                await db.Departments
                    .Where(d => d.OrganizationId == orgId && d.IsActive && d.DepartmentNameNormalized != null)
                    .Select(d => d.DepartmentNameNormalized!)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Import:Department] Existing depts loaded: {Count}", existingNames.Count);

            var insertedThisJob = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            job.Status = "IMPORTING";

            for (int i = 0; i < request.Rows.Count; i += batchSize)
            {
                await db.Entry(job).ReloadAsync();
                if (job.Status == "CANCELLED")
                {
                    _logger.LogWarning("[Import:Department] CANCELLED | JobId: {JobId} | ImportedSoFar: {Count}", job.Id, job.ImportedCount);
                    return;
                }

                await UpdateHeartbeat(db, job.Id);

                var batch = request.Rows.Skip(i).Take(batchSize).ToList();
                var batchNumber = (i / batchSize) + 1;

                _logger.LogDebug("[Import:Department] Batch {BatchNum} | Rows {From}–{To}",
                    batchNumber, i + 1, Math.Min(i + batchSize, request.Rows.Count));

                foreach (var row in batch)
                {
                    try
                    {
                        var name = GetSafeValue(row, request.Mapping, "department_name")?.Trim();
                        var normalized = name?.ToLower().Trim();

                        if (string.IsNullOrEmpty(name))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Validation", Error = "Department Name is required" });
                            _logger.LogWarning("[Import:Department] Skipped: Name empty");
                            continue;
                        }

                        if (existingNames.Contains(normalized!) || insertedThisJob.Contains(normalized!))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Duplicate", Error = $"Department '{name}' already exists for this organisation" });
                            _logger.LogWarning("[Import:Department] Skipped: Duplicate '{Name}'", name);
                            continue;
                        }

                        db.Departments.Add(new Department
                        {
                            DepartmentName = name,
                            DepartmentNameNormalized = normalized,
                            OrganizationId = orgId,
                            IsActive = true,
                            IsSystemDefault = false,
                        });

                        await db.SaveChangesAsync();

                        insertedThisJob.Add(normalized!);
                        existingNames.Add(normalized!);
                        job.ImportedCount++;
                        job.ValidatedCount++;

                        _logger.LogInformation("[Import:Department] Inserted '{Name}' | OrgId: {OrgId}", name, orgId);
                    }
                    catch (Exception ex)
                    {
                        job.FailedCount++;
                        failedRecords.Add(new { Data = row, Step = "SaveFailed", Error = ex.Message });
                        _logger.LogError(ex, "[Import:Department] Exception saving row | OrgId: {OrgId} | Error: {Error}", orgId, ex.Message);
                    }
                }

                job.CurrentStep = $"Processed {Math.Min(i + batchSize, (int)(job.TotalRecords ?? 0))}/{job.TotalRecords}";
                await db.SaveChangesAsync();

                _logger.LogDebug("[Import:Department] Batch {BatchNum} done | Imported: {I} | Failed: {F}",
                    batchNumber, job.ImportedCount, job.FailedCount);
            }

            FinaliseJob(job, failedRecords);
            await db.SaveChangesAsync();

            sw.Stop();
            _logger.LogInformation("[Import:Department] FINISHED | JobId: {JobId} | Status: {Status} | Imported: {I} | Failed: {F} | Duration: {Ms}ms",
                job.Id, job.Status, job.ImportedCount, job.FailedCount, sw.ElapsedMilliseconds);
        }

        private async Task ProcessDesignationImportAsync(PmHrmsContext db, MigrationJob job, ImportRequestModel request, int orgId)
        {
            _logger.LogInformation("[Import:Designation] Starting | JobId: {JobId} | OrgId: {OrgId} | TotalRows: {Total}",
                job.Id, orgId, request.Rows.Count);

            var sw = Stopwatch.StartNew();
            var failedRecords = new List<object>();
            int batchSize = _configuration.GetValue<int>("Migration:BatchSize", 50);

            var deptLookup = await db.Departments
                .Where(d => d.OrganizationId == orgId && d.IsActive && d.DepartmentNameNormalized != null)
                .ToDictionaryAsync(
                    d => d.DepartmentNameNormalized!,
                    d => d.DepartmentId,
                    StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Import:Designation] Dept lookup loaded: {Count}", deptLookup.Count);

            var existingDesigs = new HashSet<string>(
                await db.Designations
                    .Where(d => d.IsActive == true && d.DepartmentId != null && d.DesignationName != null)
                    .Select(d => d.DepartmentId + "|" + d.DesignationName!.ToLower().Trim())
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Import:Designation] Existing desigs loaded: {Count}", existingDesigs.Count);

            var insertedThisJob = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            job.Status = "IMPORTING";

            for (int i = 0; i < request.Rows.Count; i += batchSize)
            {
                await db.Entry(job).ReloadAsync();
                if (job.Status == "CANCELLED")
                {
                    _logger.LogWarning("[Import:Designation] CANCELLED | JobId: {JobId} | ImportedSoFar: {Count}", job.Id, job.ImportedCount);
                    return;
                }

                await UpdateHeartbeat(db, job.Id);

                var batch = request.Rows.Skip(i).Take(batchSize).ToList();
                var batchNumber = (i / batchSize) + 1;

                _logger.LogDebug("[Import:Designation] Batch {BatchNum} | Rows {From}–{To}",
                    batchNumber, i + 1, Math.Min(i + batchSize, request.Rows.Count));

                foreach (var row in batch)
                {
                    try
                    {
                        var title = GetSafeValue(row, request.Mapping, "designation_name")?.Trim();
                        var deptName = GetSafeValue(row, request.Mapping, "department_name")?.Trim();
                        var levelStr = GetSafeValue(row, request.Mapping, "hierarchy_level")?.Trim();

                        if (string.IsNullOrEmpty(title))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Validation", Error = "Designation Title is required" });
                            _logger.LogWarning("[Import:Designation] Skipped: Title empty");
                            continue;
                        }

                        if (string.IsNullOrEmpty(deptName))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Validation", Error = "Department is required" });
                            _logger.LogWarning("[Import:Designation] Skipped: Dept empty for '{Title}'", title);
                            continue;
                        }

                        if (!deptLookup.TryGetValue(deptName.ToLower(), out var deptId))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Validation", Error = $"Department '{deptName}' not found. Create it first or check spelling." });
                            _logger.LogWarning("[Import:Designation] Skipped: Dept '{Dept}' not in lookup | OrgId: {OrgId}", deptName, orgId);
                            continue;
                        }

                        var dupeKey = $"{deptId}|{title.ToLower()}";
                        if (existingDesigs.Contains(dupeKey) || insertedThisJob.Contains(dupeKey))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Duplicate", Error = $"Designation '{title}' already exists in department '{deptName}'" });
                            _logger.LogWarning("[Import:Designation] Skipped: Duplicate '{Title}' in '{Dept}'", title, deptName);
                            continue;
                        }

                        int hierarchyLevel = 10;
                        if (!string.IsNullOrEmpty(levelStr) && int.TryParse(levelStr, out int parsedLevel))
                            hierarchyLevel = parsedLevel;

                        db.Designations.Add(new Designation
                        {
                            DesignationName = title,
                            DepartmentId = deptId,
                            HierarchyLevel = hierarchyLevel,
                            IsActive = true,
                            IsSystemDefault = false,
                        });

                        await db.SaveChangesAsync();

                        insertedThisJob.Add(dupeKey);
                        existingDesigs.Add(dupeKey);
                        job.ImportedCount++;
                        job.ValidatedCount++;

                        _logger.LogInformation("[Import:Designation] Inserted '{Title}' in '{Dept}' (DeptId: {DeptId}) | Level: {Level}",
                            title, deptName, deptId, hierarchyLevel);
                    }
                    catch (Exception ex)
                    {
                        job.FailedCount++;
                        failedRecords.Add(new { Data = row, Step = "SaveFailed", Error = ex.Message });
                        _logger.LogError(ex, "[Import:Designation] Exception | OrgId: {OrgId} | Error: {Error}", orgId, ex.Message);
                    }
                }

                job.CurrentStep = $"Processed {Math.Min(i + batchSize, (int)(job.TotalRecords ?? 0))}/{job.TotalRecords}";
                await db.SaveChangesAsync();

                _logger.LogDebug("[Import:Designation] Batch {BatchNum} done | Imported: {I} | Failed: {F}",
                    batchNumber, job.ImportedCount, job.FailedCount);
            }

            FinaliseJob(job, failedRecords);
            await db.SaveChangesAsync();

            sw.Stop();
            _logger.LogInformation("[Import:Designation] FINISHED | JobId: {JobId} | Status: {Status} | Imported: {I} | Failed: {F} | Duration: {Ms}ms",
                job.Id, job.Status, job.ImportedCount, job.FailedCount, sw.ElapsedMilliseconds);
        }

        private async Task ProcessRoleImportAsync(PmHrmsContext db, MigrationJob job, ImportRequestModel request, int orgId)
        {
            _logger.LogInformation("[Import:OrgRole] Starting | JobId: {JobId} | OrgId: {OrgId} | TotalRows: {Total}",
                job.Id, orgId, request.Rows.Count);

            var sw = Stopwatch.StartNew();
            var failedRecords = new List<object>();
            int batchSize = _configuration.GetValue<int>("Migration:BatchSize", 50);

            var existingNames = new HashSet<string>(
                await db.OrgRoles
                    .Where(r => r.OrgId == orgId && r.Name != null)
                    .Select(r => r.Name!.ToLower().Trim())
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("[Import:OrgRole] Existing roles loaded: {Count}", existingNames.Count);

            var insertedThisJob = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            job.Status = "IMPORTING";

            for (int i = 0; i < request.Rows.Count; i += batchSize)
            {
                await db.Entry(job).ReloadAsync();
                if (job.Status == "CANCELLED")
                {
                    _logger.LogWarning("[Import:OrgRole] CANCELLED | JobId: {JobId} | ImportedSoFar: {Count}", job.Id, job.ImportedCount);
                    return;
                }

                await UpdateHeartbeat(db, job.Id);

                var batch = request.Rows.Skip(i).Take(batchSize).ToList();
                var batchNumber = (i / batchSize) + 1;

                _logger.LogDebug("[Import:OrgRole] Batch {BatchNum} | Rows {From}–{To}",
                    batchNumber, i + 1, Math.Min(i + batchSize, request.Rows.Count));

                foreach (var row in batch)
                {
                    try
                    {
                        var name = GetSafeValue(row, request.Mapping, "role_name")?.Trim();

                        if (string.IsNullOrEmpty(name))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Validation", Error = "Role Name is required" });
                            _logger.LogWarning("[Import:OrgRole] Skipped: Name empty");
                            continue;
                        }

                        var normalizedName = name.ToLower().Trim();
                        if (existingNames.Contains(normalizedName) || insertedThisJob.Contains(normalizedName))
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Data = row, Step = "Duplicate", Error = $"Role '{name}' already exists for this organisation" });
                            _logger.LogWarning("[Import:OrgRole] Skipped: Duplicate role '{Name}'", name);
                            continue;
                        }

                        db.OrgRoles.Add(new OrgRole { OrgId = orgId, Name = name });
                        await db.SaveChangesAsync();

                        insertedThisJob.Add(normalizedName);
                        existingNames.Add(normalizedName);
                        job.ImportedCount++;
                        job.ValidatedCount++;

                        _logger.LogInformation("[Import:OrgRole] Inserted role '{Name}' | OrgId: {OrgId}", name, orgId);
                    }
                    catch (Exception ex)
                    {
                        job.FailedCount++;
                        failedRecords.Add(new { Data = row, Step = "SaveFailed", Error = ex.Message });
                        _logger.LogError(ex, "[Import:OrgRole] Exception | OrgId: {OrgId} | Error: {Error}", orgId, ex.Message);
                    }
                }

                job.CurrentStep = $"Processed {Math.Min(i + batchSize, (int)(job.TotalRecords ?? 0))}/{job.TotalRecords}";
                await db.SaveChangesAsync();

                _logger.LogDebug("[Import:OrgRole] Batch {BatchNum} done | Imported: {I} | Failed: {F}",
                    batchNumber, job.ImportedCount, job.FailedCount);
            }

            FinaliseJob(job, failedRecords);
            await db.SaveChangesAsync();

            sw.Stop();
            _logger.LogInformation("[Import:OrgRole] FINISHED | JobId: {JobId} | Status: {Status} | Imported: {I} | Failed: {F} | Duration: {Ms}ms",
                job.Id, job.Status, job.ImportedCount, job.FailedCount, sw.ElapsedMilliseconds);
        }

        // ====================================================================================
        // 5. EMPLOYEE IMPORT (Main flow)
        // ====================================================================================

        public async Task ProcessEmployeeImportAsync(Guid jobId, ImportRequestModel request, int orgId)
        {
            _logger.LogInformation("[Import:Employee] Starting | JobId: {JobId} | OrgId: {OrgId} | TotalRows: {Total}",
                jobId, orgId, request.Rows.Count);

            var totalSw = Stopwatch.StartNew();

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PmHrmsContext>();

            db.CurrentOrgId = orgId;
            _logger.LogDebug("[Import:Employee] Context CurrentOrgId set to {OrgId} | JobId: {JobId}", orgId, jobId);

            var bulkService = scope.ServiceProvider.GetRequiredService<IBulkEmployeeService>();
            _logger.LogDebug("[Import:Employee] IBulkEmployeeService resolved from inner scope | JobId: {JobId}", jobId);

            var job = await db.MigrationJobs.FindAsync(jobId);
            if (job == null)
            {
                _logger.LogError("[Import:Employee] Job NOT FOUND | JobId: {JobId}", jobId);
                return;
            }

            try
            {
                job.Status = "SCANNING";
                job.StartedAt = DateTime.Now;
                job.MappingJson = JsonSerializer.Serialize(request.Mapping, _jsonOptions);
                await UpdateHeartbeat(db, jobId);

                _logger.LogDebug("[Import:Employee] Job status → SCANNING | JobId: {JobId}", jobId);

                var failedRecords = new List<object>();
                int batchSize = _configuration.GetValue<int>("Migration:BatchSize", 20);

                _logger.LogDebug("[Import:Employee] BatchSize: {BatchSize}", batchSize);

                // --- PHASE 1: Collect master data ---
                job.Status = "PREPARING_MASTERS";
                _logger.LogInformation("[Import:Employee] Phase 1: Collecting master data | JobId: {JobId}", jobId);

                var deptNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var desigPairs = new HashSet<(string Dept, string Desig)>();

                foreach (var row in request.Rows)
                {
                    var dept = GetSafeValue(row, request.Mapping, "department_name")?.Trim();
                    var desig = GetSafeValue(row, request.Mapping, "designation_name")?.Trim();
                    if (!string.IsNullOrWhiteSpace(dept)) deptNames.Add(dept);
                    if (!string.IsNullOrWhiteSpace(dept) && !string.IsNullOrWhiteSpace(desig))
                        desigPairs.Add((dept.ToLower(), desig.ToLower()));
                }

                _logger.LogInformation("[Import:Employee] Master data extracted | UniqueDepts: {D} | UniqueDesigPairs: {Ds}",
                    deptNames.Count, desigPairs.Count);

                // FIX: Wrap PREPARING_MASTERS SaveChangesAsync with a timeout so a SQL lock
                // from a previously cancelled/zombie job cannot hang this thread indefinitely.
                // CommandTimeout(30) in Program.cs is the first line of defence; this CTS
                // is a belt-and-suspenders guard at the application layer.
                job.Status = "PREPARING_MASTERS";
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                {
                    try
                    {
                        await db.SaveChangesAsync(cts.Token);
                        _logger.LogInformation("[Import:Employee] Phase 1: Status saved as PREPARING_MASTERS | JobId: {JobId}", jobId);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogError("[Import:Employee] TIMEOUT saving PREPARING_MASTERS status — possible SQL lock | JobId: {JobId}", jobId);
                        throw; // Caught by outer catch → marks job FAILED
                    }
                }

                db.Entry(job).State = EntityState.Detached;
                job = null!;
                _logger.LogDebug("[Import:Employee] Job entity detached from context | JobId: {JobId}", jobId);

                var masterSw = Stopwatch.StartNew();
                var (deptCache, desigCache) = await EnsureDepartmentsAndDesignationsAsync(db, orgId, deptNames, desigPairs);
                masterSw.Stop();

                _logger.LogInformation("[Import:Employee] Master data resolved | DeptCache: {D} | DesigCache: {Ds} | Duration: {Ms}ms",
                    deptCache.Count, desigCache.Count, masterSw.ElapsedMilliseconds);

                // --- PHASE 2: Import rows ---
                job = await db.MigrationJobs.FindAsync(jobId);
                if (job == null)
                {
                    _logger.LogError("[Import:Employee] Job disappeared after master data phase | JobId: {JobId}", jobId);
                    return;
                }

                _logger.LogDebug("[Import:Employee] Job re-fetched after PREPARING_MASTERS | JobId: {JobId} | Status: {Status}", jobId, job.Status);

                job.Status = "IMPORTING";
                _logger.LogInformation("[Import:Employee] Phase 2: Importing rows | JobId: {JobId}", jobId);

                var defaultOrgRole = await db.OrgRoles.FirstOrDefaultAsync(r => r.OrgId == orgId && r.Name == "Employee");
                if (defaultOrgRole == null)
                {
                    _logger.LogError("[Import:Employee] Default OrgRole 'Employee' NOT FOUND | OrgId: {OrgId}", orgId);
                    throw new InvalidOperationException("Default org role 'Employee' was not found for this organisation.");
                }

                _logger.LogDebug("[Import:Employee] Default OrgRole resolved | OrgRoleId: {Id}", defaultOrgRole.OrgRoleId);

                var existingEmails = new HashSet<string>(
                    await db.Employees
                        .Where(e => e.OrganizationId == orgId && e.OfficialEmail != null)
                        .Select(e => e.OfficialEmail!.ToLower())
                        .ToListAsync());

                _logger.LogDebug("[Import:Employee] Existing emails snapshot: {Count}", existingEmails.Count);

                for (int i = 0; i < request.Rows.Count; i += batchSize)
                {
                    await db.Entry(job).ReloadAsync();
                    if (job.Status == "CANCELLED")
                    {
                        _logger.LogWarning("[Import:Employee] CANCELLED | JobId: {JobId} | ImportedSoFar: {Count}", jobId, job.ImportedCount);
                        return;
                    }

                    await UpdateHeartbeat(db, jobId);

                    var batch = request.Rows.Skip(i).Take(batchSize).ToList();
                    var batchNumber = (i / batchSize) + 1;
                    var validEmployees = new List<EmployeeModel>();
                    var batchEmails = new HashSet<string>();
                    var processedRows = new List<MigrationJobRow>();

                    _logger.LogDebug("[Import:Employee] Batch {BatchNum} | Rows {From}–{To}",
                        batchNumber, i + 1, Math.Min(i + batchSize, request.Rows.Count));

                    var batchSw = Stopwatch.StartNew();

                    foreach (var row in batch)
                    {
                        try
                        {
                            var email = GetSafeValue(row, request.Mapping, "official_email")?.ToLower();
                            if (string.IsNullOrWhiteSpace(email))
                            {
                                _logger.LogDebug("[Import:Employee] Row skipped: Email empty");
                                continue;
                            }

                            var rowHash = ComputeRowHash(email, GetSafeValue(row, request.Mapping, "employee_code") ?? "", orgId);

                            if (await IsRowProcessed(db, jobId, rowHash))
                            {
                                _logger.LogDebug("[Import:Employee] Row skipped (idempotency hit): '{Email}'", email);
                                continue;
                            }

                            if (!batchEmails.Add(email))
                            {
                                _logger.LogWarning("[Import:Employee] Duplicate email in batch: '{Email}'", email);
                                throw new Exception("Duplicate email in batch.");
                            }

                            if (existingEmails.Contains(email))
                            {
                                _logger.LogWarning("[Import:Employee] Email already in DB: '{Email}'", email);
                                throw new Exception("Email already exists in system.");
                            }

                            var dName = GetSafeValue(row, request.Mapping, "department_name")?.Trim();
                            var dsName = GetSafeValue(row, request.Mapping, "designation_name")?.Trim();

                            if (string.IsNullOrEmpty(dName)) throw new Exception("Department is required.");
                            if (string.IsNullOrEmpty(dsName)) throw new Exception("Designation is required.");

                            if (!deptCache.TryGetValue(dName, out var deptId) || deptId == 0)
                            {
                                _logger.LogError("[Import:Employee] DEPT CACHE MISS for '{Dept}' | JobId: {JobId}", dName, jobId);
                                throw new Exception($"Department '{dName}' could not be resolved.");
                            }

                            var desigKey = $"{deptId}_{dsName.ToLower()}";
                            if (!desigCache.TryGetValue(desigKey, out var desigId) || desigId == 0)
                            {
                                _logger.LogError("[Import:Employee] DESIG CACHE MISS for key '{Key}' | JobId: {JobId}", desigKey, jobId);
                                throw new Exception($"Designation '{dsName}' could not be resolved for department '{dName}'.");
                            }

                            validEmployees.Add(new EmployeeModel
                            {
                                OrganizationId = orgId,
                                FirstName = GetSafeValue(row, request.Mapping, "first_name") ?? "",
                                LastName = GetSafeValue(row, request.Mapping, "last_name"),
                                OfficialEmail = email,
                                EmployeeCode = GetSafeValue(row, request.Mapping, "employee_code") ?? "",
                                PhoneNumber = SanitizePhone(GetSafeValue(row, request.Mapping, "phone_number")),
                                WorkMode = NormalizeWorkMode(GetSafeValue(row, request.Mapping, "work_mode")),
                                DateOfJoining = ParseDateOfJoining(GetSafeValue(row, request.Mapping, "date_of_joining")),
                                DepartmentId = deptId,
                                DesignationId = desigId,
                                OrgRoleId = (byte)defaultOrgRole.OrgRoleId,
                                SystemRoleId = (byte)2,
                                EmploymentStatus = "FullTime"
                            });

                            processedRows.Add(new MigrationJobRow
                            {
                                JobId = jobId,
                                RowHash = rowHash,
                                Status = "PROCESSED",
                                CreatedAt = DateTime.Now
                            });

                            job.ValidatedCount++;
                            _logger.LogDebug("[Import:Employee] Row queued | Email: {Email} | Dept: {Dept} | Desig: {Desig}", email, dName, dsName);
                        }
                        catch (Exception ex)
                        {
                            job.FailedCount++;
                            failedRecords.Add(new { Step = "RowProcessing", Error = ex.InnerException?.Message ?? ex.Message });
                            _logger.LogError("[Import:Employee] Row error: {Error}", ex.InnerException?.Message ?? ex.Message);
                        }
                    }

                    batchSw.Stop();
                    _logger.LogDebug("[Import:Employee] Batch {BatchNum} loop done | Valid: {V} | Failed: {F} | LoopTime: {Ms}ms",
                        batchNumber, validEmployees.Count, job.FailedCount, batchSw.ElapsedMilliseconds);

                    if (validEmployees.Any())
                    {
                        try
                        {
                            var bulkSw = Stopwatch.StartNew();
                            await bulkService.BulkInsertAsync(
                                orgId, validEmployees, 2, defaultOrgRole.OrgRoleId, job.SharedPassword);
                            bulkSw.Stop();

                            job.ImportedCount += validEmployees.Count;

                            _logger.LogInformation("[Import:Employee] BulkInsert OK | Batch: {BatchNum} | Inserted: {Count} | BulkDuration: {Ms}ms",
                                batchNumber, validEmployees.Count, bulkSw.ElapsedMilliseconds);

                            if (processedRows.Any())
                                db.Set<MigrationJobRow>().AddRange(processedRows);
                        }
                        catch (Exception ex)
                        {
                            job.FailedCount += validEmployees.Count;
                            failedRecords.Add(new { Step = "BulkInsert", Error = ex.InnerException?.Message ?? ex.Message });
                            _logger.LogError(ex, "[Import:Employee] BulkInsert FAILED | Batch: {BatchNum} | LostRows: {Count} | Error: {Error}",
                                batchNumber, validEmployees.Count, ex.InnerException?.Message ?? ex.Message);

                               foreach (var entry in db.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added)
                                    .ToList())
                                {
                                    entry.State = EntityState.Detached;
                                } 
                        }
                    }
                    else
                    {
                        _logger.LogDebug("[Import:Employee] Batch {BatchNum}: No valid employees to insert", batchNumber);
                    }

                    job.CurrentStep = $"Processed {i + batch.Count}/{job.TotalRecords}";
                    await db.SaveChangesAsync();

                    _logger.LogDebug("[Import:Employee] Batch {BatchNum} committed | TotalImported: {I} | TotalFailed: {F}",
                        batchNumber, job.ImportedCount, job.FailedCount);
                }

                job.Status = (job.FailedCount > 0 && job.ImportedCount > 0) ? "PARTIAL_SUCCESS"
                                : (job.ImportedCount == 0) ? "FAILED"
                                : "COMPLETED";
                job.CompletedAt = DateTime.Now;
                job.ErrorLog = failedRecords.Any() ? JsonSerializer.Serialize(failedRecords, _jsonOptions) : null;
                await db.SaveChangesAsync();

                totalSw.Stop();
                _logger.LogInformation("[Import:Employee] COMPLETE | JobId: {JobId} | Status: {Status} | Imported: {I} | Failed: {F} | TotalDuration: {Ms}ms",
                    jobId, job.Status, job.ImportedCount, job.FailedCount, totalSw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                totalSw.Stop();
                _logger.LogCritical(ex, "[Import:Employee] FATAL ERROR | JobId: {JobId} | Duration: {Ms}ms | Error: {Error}",
                    jobId, totalSw.ElapsedMilliseconds, ex.Message);

                job = await db.MigrationJobs.FindAsync(jobId);
                if (job != null)
                {
                    job.Status = "FAILED";
                    job.ErrorLog = ex.Message;
                    await db.SaveChangesAsync();
                }
            }
        }

        // ====================================================================================
        // 6. MASTER DATA — SAFE ONE-BY-ONE RESOLUTION
        // ====================================================================================

        private async Task<(Dictionary<string, int> deptDict, Dictionary<string, int> desigDict)>
            EnsureDepartmentsAndDesignationsAsync(
                PmHrmsContext db,
                int orgId,
                HashSet<string> deptNames,
                HashSet<(string Dept, string Desig)> desigPairs)
        {
            _logger.LogInformation("[MasterData] Starting resolution | OrgId: {OrgId} | Depts: {D} | DesigPairs: {Ds}",
                orgId, deptNames.Count, desigPairs.Count);

            var sw = Stopwatch.StartNew();
            var deptDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var deptName in deptNames)
            {
                var normalized = deptName.Trim().ToLowerInvariant();

                var existing = await db.Departments
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.OrganizationId == orgId && d.DepartmentNameNormalized == normalized);

                if (existing != null)
                {
                    deptDict[deptName] = existing.DepartmentId;
                    _logger.LogDebug("[MasterData:Dept] Found existing '{Dept}' → DeptId: {Id}", deptName, existing.DepartmentId);
                    continue;
                }

                _logger.LogInformation("[MasterData:Dept] '{Dept}' not in DB — inserting | OrgId: {OrgId}", deptName, orgId);

                var newDept = new Department
                {
                    DepartmentName = deptName.Trim(),
                    DepartmentNameNormalized = normalized,
                    OrganizationId = orgId,
                    IsActive = true,
                    IsSystemDefault = false
                };

                db.Departments.Add(newDept);

                try
                {
                    await db.SaveChangesAsync();
                    deptDict[deptName] = newDept.DepartmentId;
                    _logger.LogInformation("[MasterData:Dept] Inserted '{Dept}' → DeptId: {Id}", deptName, newDept.DepartmentId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "[MasterData:Dept] Concurrent insert conflict for '{Dept}' — re-fetching from DB", deptName);

                    db.Entry(newDept).State = EntityState.Detached;

                    var fresh = await db.Departments
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.OrganizationId == orgId && d.DepartmentNameNormalized == normalized);

                    if (fresh != null)
                    {
                        deptDict[deptName] = fresh.DepartmentId;
                        _logger.LogInformation("[MasterData:Dept] Recovered '{Dept}' after conflict → DeptId: {Id}", deptName, fresh.DepartmentId);
                    }
                    else
                    {
                        _logger.LogError("[MasterData:Dept] CRITICAL: Could not resolve '{Dept}' even after retry | OrgId: {OrgId}", deptName, orgId);
                    }
                }
            }

            foreach (var deptName in deptNames)
            {
                if (!deptDict.TryGetValue(deptName, out var id) || id == 0)
                {
                    _logger.LogError("[MasterData:Dept] Unresolved dept '{Dept}' — will throw | OrgId: {OrgId}", deptName, orgId);
                    throw new InvalidOperationException(
                        $"Department '{deptName}' could not be resolved. Please check the name or create it manually first.");
                }
            }

            _logger.LogInformation("[MasterData:Dept] All {Count} departments resolved", deptDict.Count);

            // --- Designations ---
            var desigDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var existingDesigs = await db.Designations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(d => d.IsActive == true && d.DepartmentId != null && d.DesignationName != null)
                .ToListAsync();

            foreach (var d in existingDesigs)
                desigDict[$"{d.DepartmentId}_{d.DesignationName!.Trim().ToLower()}"] = d.DesignationId;

            _logger.LogDebug("[MasterData:Desig] Loaded {Count} existing designations from DB", existingDesigs.Count);

            foreach (var (deptName, desigName) in desigPairs)
            {
                if (!deptDict.TryGetValue(deptName, out var deptId))
                {
                    _logger.LogWarning("[MasterData:Desig] Skipping '{Desig}' — dept '{Dept}' not in cache", desigName, deptName);
                    continue;
                }

                var key = $"{deptId}_{desigName.ToLower()}";
                if (desigDict.ContainsKey(key))
                {
                    _logger.LogDebug("[MasterData:Desig] '{Desig}' already exists for DeptId {DeptId} — skipping insert", desigName, deptId);
                    continue;
                }

                _logger.LogInformation("[MasterData:Desig] Inserting '{Desig}' under DeptId {DeptId}", desigName, deptId);

                var newDesig = new Designation
                {
                    DepartmentId = deptId,
                    DesignationName = desigName,
                    IsActive = true,
                    HierarchyLevel = 10
                };

                db.Designations.Add(newDesig);

                try
                {
                    await db.SaveChangesAsync();
                    desigDict[key] = newDesig.DesignationId;
                    _logger.LogInformation("[MasterData:Desig] Inserted '{Desig}' → DesigId: {Id}", desigName, newDesig.DesignationId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "[MasterData:Desig] Concurrent insert conflict for '{Desig}' — re-fetching", desigName);

                    db.Entry(newDesig).State = EntityState.Detached;

                    var fresh = await db.Designations
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(d => d.DepartmentId == deptId
                            && d.DesignationName != null
                            && d.DesignationName.ToLower().Trim() == desigName.ToLower());

                    if (fresh != null)
                    {
                        desigDict[key] = fresh.DesignationId;
                        _logger.LogInformation("[MasterData:Desig] Recovered '{Desig}' after conflict → DesigId: {Id}", desigName, fresh.DesignationId);
                    }
                    else
                    {
                        _logger.LogError("[MasterData:Desig] CRITICAL: Could not resolve '{Desig}' for DeptId {DeptId}", desigName, deptId);
                    }
                }
            }

            sw.Stop();
            _logger.LogInformation("[MasterData] Done | Depts: {D} | Desigs: {Ds} | Duration: {Ms}ms",
                deptDict.Count, desigDict.Count, sw.ElapsedMilliseconds);

            return (deptDict, desigDict);
        }

        // ====================================================================================
        // 7. HELPERS
        // ====================================================================================

        private void FinaliseJob(MigrationJob job, List<object> failedRecords)
        {
            var prevStatus = job.Status;
            job.Status = job.FailedCount > 0 && job.ImportedCount > 0 ? "PARTIAL_SUCCESS"
                            : job.ImportedCount == 0 ? "FAILED"
                            : "COMPLETED";
            job.CompletedAt = DateTime.Now;
            job.ErrorLog = failedRecords.Any() ? JsonSerializer.Serialize(failedRecords, _jsonOptions) : null;

            _logger.LogInformation("[Job:Finalise] JobId: {JobId} | {Prev} → {Status} | Imported: {I} | Failed: {F}",
                job.Id, prevStatus, job.Status, job.ImportedCount, job.FailedCount);
        }

        private async Task UpdateHeartbeat(PmHrmsContext db, Guid jobId)
        {
            var job = await db.MigrationJobs.FindAsync(jobId);
            if (job != null)
            {
                job.LastHeartbeat = DateTime.Now;
                try
                {
                    await db.SaveChangesAsync();
                    _logger.LogDebug("[Heartbeat] Updated | JobId: {JobId} | At: {Time}", jobId, job.LastHeartbeat);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[Heartbeat] Save failed (dirty context) | JobId: {JobId} | Error: {Error}. Detaching added entries to recover.",
                        jobId, ex.Message);
                    var added = db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
                    _logger.LogDebug("[Heartbeat] Detaching {Count} Added entries", added.Count);
                    foreach (var e in added) e.State = EntityState.Detached;
                    await db.SaveChangesAsync();
                    _logger.LogDebug("[Heartbeat] Recovered | JobId: {JobId}", jobId);
                }
            }
            else
            {
                _logger.LogWarning("[Heartbeat] Job NOT FOUND during heartbeat | JobId: {JobId}", jobId);
            }
        }

        public async Task<(bool Success, string Message)> CancelJobAsync(Guid jobId)
        {
            _logger.LogInformation("[Job:Cancel] Cancel requested | JobId: {JobId}", jobId);

            var job = await _context.MigrationJobs.FindAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning("[Job:Cancel] Job not found | JobId: {JobId}", jobId);
                return (false, "Not Found");
            }

            if (job.Status == "COMPLETED" || job.Status == "FAILED")
            {
                _logger.LogWarning("[Job:Cancel] Cannot cancel — already terminal | JobId: {JobId} | Status: {Status}", jobId, job.Status);
                return (false, "Already finished");
            }

            job.Status = "CANCELLED";
            job.CompletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("[Job:Cancel] Cancelled | JobId: {JobId}", jobId);
            return (true, "Cancelled");
        }

        public async Task<MigrationJob?> GetJobByIdAsync(Guid jobId)
        {
            _logger.LogDebug("[Job:Get] Fetching | JobId: {JobId}", jobId);
            return await _context.MigrationJobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == jobId);
        }

        public async Task<List<MigrationJob>> GetHistoryAsync(int orgId, string entityType)
        {
            var normalizedEntityType = NormalizeEntityType(entityType);
            _logger.LogDebug("[Job:History] Fetching | OrgId: {OrgId} | EntityType: {EntityType}", orgId, normalizedEntityType);

            var list = await _context.MigrationJobs
                .AsNoTracking()
                .Where(j => j.OrgId == orgId && j.EntityType == normalizedEntityType)
                .OrderByDescending(j => j.CreatedAt)
                .Take(50)
                .ToListAsync();

            _logger.LogDebug("[Job:History] Found {Count} jobs | OrgId: {OrgId}", list.Count, orgId);
            return list;
        }

        private string? GetSafeValue(Dictionary<string, string?> r, Dictionary<string, string> m, string k)
        {
            if (r.TryGetValue(k, out var val)) return val?.Trim();
            if (m.TryGetValue(k, out var col) && r.TryGetValue(col, out var val2)) return val2?.Trim();
            return null;
        }

        private double GetSimilarity(string s, string t)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(t)) return 0;
            s = s.ToLower().Trim(); t = t.ToLower().Trim();
            if (s == t) return 1.0;
            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                                       d[i - 1, j - 1] + (t[j - 1] == s[i - 1] ? 0 : 1));
            return 1.0 - ((double)d[n, m] / Math.Max(n, m));
        }

        private double AnalyzeContent(List<string> values, SystemField field)
        {
            if (!values.Any()) return 0;
            var nonNull = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            if (nonNull.Count == 0) return 0;
            int match = 0;
            foreach (var v in nonNull)
            {
                bool isMatch = false;
                if (!string.IsNullOrEmpty(field.ValidationType))
                {
                    isMatch = field.ValidationType switch
                    {
                        "Email" => Regex.IsMatch(v, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                        "Phone" => Regex.IsMatch(Regex.Replace(v, @"[^\d]", ""), @"^\d{10,15}$"),
                        _ => false
                    };
                }
                if (!isMatch && field.Keywords != null)
                {
                    var keys = field.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim());
                    if (keys.Any(k => v.Contains(k, StringComparison.OrdinalIgnoreCase))) isMatch = true;
                }
                if (isMatch) match++;
            }
            return (double)match / nonNull.Count;
        }

        public string? GeneralHeal(string value, string type)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return type switch
            {
                "Phone" => Regex.Replace(value, @"[^0-9]", ""),
                "Email" => value.ToLower().Trim().Replace(" ", ""),
                "UpperCase" => value.ToUpper().Trim(),
                "ProperCase" => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower()),
                _ => value.Trim()
            };
        }

        public byte[] GenerateExcelTemplate(List<string> headers)
        {
            _logger.LogDebug("[Template] Generating | Headers: {Headers}", string.Join(", ", headers));
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            _logger.LogDebug("[Template] Generated | Size: {Bytes} bytes", stream.Length);
            return stream.ToArray();
        }

        private bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private string? SanitizePhone(string? phone) => string.IsNullOrWhiteSpace(phone) ? null : Regex.Replace(phone, @"[^0-9+\-() ]", "").Trim();
        private string NormalizeWorkMode(string? w) => w?.ToUpper().Trim() switch { "REMOTE" => "REMOTE", "WFH" => "REMOTE", "HYBRID" => "HYBRID", _ => "OFFICE" };
        private string GenerateStrongPassword() => "Pm@" + Guid.NewGuid().ToString()[..8];
        private string ComputeRowHash(string e, string c, int o) => Convert.ToHexString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes($"{e}|{c}|{o}".ToLower())));

        private async Task<bool> IsRowProcessed(PmHrmsContext db, Guid j, string h) =>
            await db.Set<MigrationJobRow>().AnyAsync(r => r.JobId == j && r.RowHash == h && r.Status == "PROCESSED");

        private DateOnly ParseDateOfJoining(string? d)
        {
            if (string.IsNullOrWhiteSpace(d)) return DateOnly.FromDateTime(DateTime.Now);
            var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy/MM/dd" };
            foreach (var f in formats)
                if (DateTime.TryParseExact(d, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime r))
                    return DateOnly.FromDateTime(r);
            return DateTime.TryParse(d, out DateTime fb) ? DateOnly.FromDateTime(fb) : DateOnly.FromDateTime(DateTime.Now);
        }

        private static string NormalizeEntityType(string? entityType) =>
            entityType?.Trim().ToLowerInvariant() switch
            {
                "employee" or "employees" => "Employee",
                "department" or "departments" => "Department",
                "designation" or "designations" => "Designation",
                "orgrole" or "role" or "roles" => "OrgRole",
                _ => entityType?.Trim() ?? string.Empty
            };
    }
}