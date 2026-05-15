using Hangfire;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public class PayrollService : IPayrollService
    {
        private const byte DefaultAutoRunDay = 5;
        private const byte DefaultPayrollCutoffDay = 25;
        private const byte DefaultPayslipGenerateDay = 7;
        private const string DefaultCurrency = "INR";
        private const string DefaultRoundingRule = "Round";
        private static readonly string[] AllowedRoundingRules = ["Floor", "Ceil", "Round"];

        private readonly PmHrmsContext _context;

        public PayrollService(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<int> CreatePayrollRunAsync(CreatePayrollRequest model)
        {
            var now = DateTime.UtcNow;

            var runType = Enum.TryParse<PayrollRunType>(model.RunType, true, out var parsedType) 
            ? parsedType 
            : PayrollRunType.Regular;

            var triggeredByName = NormalizePayrollTrigger(model.TriggeredBy);


            var payrollRun = new PayrollRun
            {
                OrgId = model.OrgId,
                PayrollMonth = model.PayrollMonth ?? (byte)(model.StartDate?.Month ?? 0),
                PayrollYear = model.PayrollYear ?? (short)(model.StartDate?.Year ?? 0),
                StartDate = model.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
                EndDate = model.EndDate ?? DateOnly.FromDateTime(DateTime.Today),
                Status = "Pending",
                RunType = runType,
                IsLocked = false,
                ApprovalStatus = "Pending",
                TriggeredBy = triggeredByName,
                ScheduledRunDate = model.ScheduledRunDate,
                IsRecalculated = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.PayrollRuns.Add(payrollRun);
            await _context.SaveChangesAsync();

            return payrollRun.Id;
        }

        public Task<bool> StartPayrollAsync(int payrollRunId)
        {
            var run = _context.PayrollRuns.FirstOrDefault(r => r.Id == payrollRunId);
            if (run == null) return Task.FromResult(false);
            if (run.IsLocked || run.Status == "Running") return Task.FromResult(false);

            BackgroundJob.Enqueue<PmHrmsAPI.PmHrmsBAL.Jobs.PayrollJob>(job => job.RunPayrollAsync(payrollRunId));
            return Task.FromResult(true);
        }

        public async Task<PayrollRunResponse> GetPayrollRunAsync(int payrollRunId)
        {
            var run = await _context.PayrollRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == payrollRunId);
            if (run == null) return null!;

            return new PayrollRunResponse
            {
                Id = run.Id,
                OrgId = run.OrgId,
                PayrollMonth = run.PayrollMonth,
                PayrollYear = run.PayrollYear,
                StartDate = run.StartDate,
                EndDate = run.EndDate,
                Status = run.Status,
                RunType = run.RunType,
                IsLocked = run.IsLocked,
                ActualRunStart = run.ActualRunStart,
                ActualRunEnd = run.ActualRunEnd,
                TotalEmployees = run.TotalEmployees,
                TotalNetPayable = run.TotalNetPayable,
                TriggeredBy = run.TriggeredBy,
                ErrorMessage = run.ErrorMessage,
                CreatedAt = run.CreatedAt,
                UpdatedAt = run.UpdatedAt
            };
        }

        public async Task<List<PayrollRunListResponse>> GetPayrollRunsAsync(int orgId)
        {
            var runs = await _context.PayrollRuns
                .AsNoTracking()
                .Where(r => r.OrgId == orgId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return runs.Select(r => new PayrollRunListResponse
            {
                Id = r.Id,
                Period = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(r.PayrollMonth) + " " + r.PayrollYear,
                 StartDate     = r.StartDate,      
                 EndDate       = r.EndDate,        
                 Status        = r.Status,
                 TriggeredBy   = r.TriggeredBy,   
                 TotalEmployees   = r.TotalEmployees ?? 0,
                 TotalNetPayable  = r.TotalNetPayable ?? 0m,
                 CreatedAt     = r.CreatedAt
            }).ToList();
        }

        public async Task<List<EmployeePayrollResponse>> GetEmployeePayrollsAsync(int payrollRunId)
        {
            var payrolls = await _context.EmployeePayrolls
                .AsNoTracking()
                .Where(p => p.PayrollRunId == payrollRunId)
                .OrderBy(p => p.EmployeeId)
                .ToListAsync();

            var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
            var employees = await _context.Employees
                .AsNoTracking()
                .Where(e => employeeIds.Contains(e.EmployeeId))
                .ToDictionaryAsync(e => e.EmployeeId, e => e.FirstName + " " + e.LastName);

            var components = await _context.EmployeePayrollComponents
                .AsNoTracking()
                .Where(c => payrolls.Select(p => p.Id).Contains(c.EmployeePayrollId))
                .ToListAsync();

            return payrolls.Select(p => new EmployeePayrollResponse
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = employees.GetValueOrDefault(p.EmployeeId) ?? string.Empty,
                TotalWorkingDays = p.TotalWorkingDays,
                PresentDays = p.PresentDays,
                AbsentDays = p.AbsentDays,
                GrossSalary = p.GrossSalary,
                TotalEarnings = p.TotalEarnings,
                TotalDeductions = p.TotalDeductions,
                Remarks = p.Remarks,
                NetPayable = p.NetPayable,
                Status = p.Status,
                Components = components
                    .Where(c => c.EmployeePayrollId == p.Id)
                    .Select(c => new ComponentResult { Name = c.ComponentName, Amount = c.Amount, Type = c.Type })
                    .ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }



       

public async Task<EmployeePayrollResponse> GetEmployeePayrollAsync(int runId, int employeePayrollId)
{
    var payroll = await _context.EmployeePayrolls
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.PayrollRunId == runId && p.Id == employeePayrollId);

    if (payroll == null) return null!;

    var employee = await _context.Employees
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.EmployeeId == payroll.EmployeeId);

    var components = await _context.EmployeePayrollComponents
        .AsNoTracking()
        .Where(c => c.EmployeePayrollId == payroll.Id)
        .ToListAsync();


        string? bankName = null, accountHolder = null, accountNumber = null, ifsc = null;
    if (!string.IsNullOrWhiteSpace(payroll.BankAccountSnapshot))
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payroll.BankAccountSnapshot);
            var root = doc.RootElement;
            if (root.TryGetProperty("BankName", out var p1))          bankName       = p1.GetString();
            if (root.TryGetProperty("AccountHolderName", out var p2)) accountHolder  = p2.GetString();
            if (root.TryGetProperty("AccountNumber", out var p3))     accountNumber  = p3.GetString();
            if (root.TryGetProperty("IFSCCode", out var p4))          ifsc           = p4.GetString();
        }
        catch { /* malformed snapshot — silently ignore */ }
    }


    return new EmployeePayrollResponse
    {
        Id = payroll.Id,
        EmployeeId = payroll.EmployeeId,
        EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
        // Assuming your Employee table has EmployeeCode and Department. If not, map to empty string temporarily.
        EmployeeCode = employee?.EmployeeCode ?? string.Empty, 
        //DepartmentName  = employee?.Department.DepartmentName ?? string.Empty ,
        TotalWorkingDays = payroll.TotalWorkingDays,
        PresentDays = payroll.PresentDays,
        AbsentDays = payroll.AbsentDays,
        GrossSalary = payroll.GrossSalary,
        TotalEarnings = payroll.TotalEarnings,
        TotalDeductions = payroll.TotalDeductions,
        NetPayable = payroll.NetPayable,
        Status = payroll.Status,
        Remarks = payroll.Remarks,
        Components = components
            .Select(c => new ComponentResult { Name = c.ComponentName, Amount = c.Amount, Type = c.Type })
            .ToList(),

        BankName          = bankName,
        AccountHolderName = accountHolder,
        AccountNumber     = accountNumber,
        IFSCCode          = ifsc, 
        CreatedAt = payroll.CreatedAt,
        UpdatedAt = payroll.UpdatedAt
    };
}

public async Task<IEnumerable<object>> GetRunDownloadRowsAsync(int runId)
{
    // This creates an anonymous object that will serialize nicely to JSON 
    // for your Angular frontend to convert to CSV/Excel

    var payrolls = await _context.EmployeePayrolls
        .AsNoTracking()
        .Where(p => p.PayrollRunId == runId)
        .ToListAsync();

    
    var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
    var employees = await _context.Employees
        .AsNoTracking()
        .Where(e => employeeIds.Contains(e.EmployeeId))
        .ToDictionaryAsync(e => e.EmployeeId, e => e);

        var result = new List<object>();

        foreach (var p in payrolls)
    {
        // Default empty strings for bank details
        string accountHolder = "";
        string accountNumber = "";
        string bankName = "";
        string ifsc = "";

        // Safely parse the JSON snapshot
        if (!string.IsNullOrWhiteSpace(p.BankAccountSnapshot))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(p.BankAccountSnapshot);
                var root = doc.RootElement;
                if (root.TryGetProperty("AccountHolderName", out var nameProp)) accountHolder = nameProp.GetString();
                if (root.TryGetProperty("AccountNumber", out var numProp)) accountNumber = numProp.GetString();
                if (root.TryGetProperty("BankName", out var bankProp)) bankName = bankProp.GetString();
                if (root.TryGetProperty("IFSCCode", out var ifscProp)) ifsc = ifscProp.GetString();
            }
            catch { /* Ignore parse errors if snapshot is malformed */ }
        }

        var emp = employees.GetValueOrDefault(p.EmployeeId);

        // This anonymous object defines the exact columns that will appear in your Excel file
        result.Add(new
        {
            EmployeeCode = emp?.EmployeeCode ?? "",
            EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
            p.TotalWorkingDays,
            p.PresentDays,
            p.AbsentDays,
            p.LeaveDays,
            p.LopDays,
            p.GrossSalary,
            p.TotalEarnings,
            p.TotalDeductions,
            p.NetPayable,
            BankName = bankName,
            AccountHolderName = accountHolder,
            AccountNumber = accountNumber,
            IFSCCode = ifsc,
            Remarks = p.Remarks,
            Status = p.Status
        });
    }

    return result;
}



    


public async Task<PayrollConfigResponse> GetConfigurationAsync(int orgId)
{
    if (orgId <= 0)
    {
        throw new ArgumentException("A valid organization id is required.");
    }

    var config = await _context.PayrollConfigs
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.OrgId == orgId);

    if (config == null)
    {
        return BuildDefaultConfigResponse();
    }

    return MapConfigResponse(config);
}

public async Task<PayrollConfigResponse> SaveConfigurationAsync(int orgId, UpdatePayrollConfigRequest model)
{
    if (orgId <= 0)
    {
        throw new ArgumentException("A valid organization id is required.");
    }

    if (model == null)
    {
        throw new ArgumentException("Payroll configuration payload is required.");
    }

    var config = await _context.PayrollConfigs.FirstOrDefaultAsync(c => c.OrgId == orgId);
    var now = DateTime.UtcNow;
    
    if (config == null)
    {
        config = new PayrollConfig
        {
            OrgId = orgId,
            AutoRunDayOfMonth = DefaultAutoRunDay,
            IsAutoRunEnabled = false,
            PayrollCutoffDay = DefaultPayrollCutoffDay,
            PayslipGenerateDay = DefaultPayslipGenerateDay,
            Currency = DefaultCurrency,
            RoundingRule = DefaultRoundingRule,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.PayrollConfigs.Add(config);
    }

    ApplyConfiguration(config, model, now);

    await _context.SaveChangesAsync();

    return MapConfigResponse(config);
}

private static PayrollConfigResponse BuildDefaultConfigResponse()
{
    return new PayrollConfigResponse
    {
        AutoRunDay = DefaultAutoRunDay,
        AutoRunEnabled = false,
        PayrollCutoffDay = DefaultPayrollCutoffDay,
        PayslipGenerateDay = DefaultPayslipGenerateDay,
        Currency = DefaultCurrency,
        RoundingRule = DefaultRoundingRule,
        IsPfApplicable = false,
        IsEsiApplicable = false,
        IsTdsApplicable = false,
        UpdatedBy = "System"
    };
}

private static PayrollConfigResponse MapConfigResponse(PayrollConfig config)
{
    return new PayrollConfigResponse
    {
        AutoRunDay = config.AutoRunDayOfMonth == 0 ? DefaultAutoRunDay : config.AutoRunDayOfMonth,
        AutoRunEnabled = config.IsAutoRunEnabled,
        PayrollCutoffDay = config.PayrollCutoffDay == 0 ? DefaultPayrollCutoffDay : config.PayrollCutoffDay,
        PayslipGenerateDay = config.PayslipGenerateDay == 0 ? DefaultPayslipGenerateDay : config.PayslipGenerateDay,
        Currency = string.IsNullOrWhiteSpace(config.Currency) ? DefaultCurrency : config.Currency.Trim(),
        RoundingRule = string.IsNullOrWhiteSpace(config.RoundingRule) ? DefaultRoundingRule : config.RoundingRule.Trim(),
        IsPfApplicable = config.IsPfApplicable,
        PfWageLimit = config.PfWageLimit,
        PfEmployeePercentage = config.PfEmployeePercentage,
        PfEmployerPercentage = config.PfEmployerPercentage,
        IsEsiApplicable = config.IsEsiApplicable,
        EsiEmployeePercentage = config.EsiEmployeePercentage,
        EsiEmployerPercentage = config.EsiEmployerPercentage,
        IsTdsApplicable = config.IsTdsApplicable,
        TdsPercentage = config.TdsPercentage,
        CreatedAt = config.CreatedAt,
        UpdatedAt = config.UpdatedAt,
        UpdatedBy = "System"
    };
}

private static void ApplyConfiguration(PayrollConfig config, UpdatePayrollConfigRequest model, DateTime now)
{
    config.AutoRunDayOfMonth = ResolveDay(
        model.AutoRunDay,
        config.AutoRunDayOfMonth,
        DefaultAutoRunDay,
        28,
        nameof(model.AutoRunDay));
    config.IsAutoRunEnabled = model.AutoRunEnabled;
    config.PayrollCutoffDay = ResolveDay(
        model.PayrollCutoffDay,
        config.PayrollCutoffDay,
        DefaultPayrollCutoffDay,
        31,
        nameof(model.PayrollCutoffDay));
    config.PayslipGenerateDay = ResolveDay(
        model.PayslipGenerateDay,
        config.PayslipGenerateDay,
        DefaultPayslipGenerateDay,
        31,
        nameof(model.PayslipGenerateDay));
    config.Currency = ResolveCurrency(model.Currency, config.Currency);
    config.RoundingRule = ResolveRoundingRule(model.RoundingRule, config.RoundingRule);
    config.IsPfApplicable = model.IsPfApplicable;
    config.PfWageLimit = model.IsPfApplicable ? ValidateAmount(model.PfWageLimit, nameof(model.PfWageLimit)) : null;
    config.PfEmployeePercentage = model.IsPfApplicable ? ValidatePercentage(model.PfEmployeePercentage, nameof(model.PfEmployeePercentage)) : null;
    config.PfEmployerPercentage = model.IsPfApplicable ? ValidatePercentage(model.PfEmployerPercentage, nameof(model.PfEmployerPercentage)) : null;
    config.IsEsiApplicable = model.IsEsiApplicable;
    config.EsiEmployeePercentage = model.IsEsiApplicable ? ValidatePercentage(model.EsiEmployeePercentage, nameof(model.EsiEmployeePercentage)) : null;
    config.EsiEmployerPercentage = model.IsEsiApplicable ? ValidatePercentage(model.EsiEmployerPercentage, nameof(model.EsiEmployerPercentage)) : null;
    config.IsTdsApplicable = model.IsTdsApplicable;
    config.TdsPercentage = model.IsTdsApplicable ? ValidatePercentage(model.TdsPercentage, nameof(model.TdsPercentage)) : null;
    config.UpdatedAt = now;
}

private static byte ResolveDay(byte requestedValue, byte currentValue, byte fallback, byte max, string fieldName)
{
    var resolvedValue = requestedValue == 0
        ? currentValue == 0 ? fallback : currentValue
        : requestedValue;

    if (resolvedValue < 1 || resolvedValue > max)
    {
        throw new ArgumentException($"{fieldName} must be between 1 and {max}.");
    }

    return resolvedValue;
}

private static string ResolveCurrency(string? requestedValue, string? currentValue)
{
    var resolvedValue = string.IsNullOrWhiteSpace(requestedValue) ? currentValue : requestedValue;
    resolvedValue = string.IsNullOrWhiteSpace(resolvedValue) ? DefaultCurrency : resolvedValue.Trim().ToUpperInvariant();

    if (resolvedValue.Length != 3 || !resolvedValue.All(char.IsLetter))
    {
        throw new ArgumentException("Currency must be a 3-letter ISO code.");
    }

    return resolvedValue;
}

private static string ResolveRoundingRule(string? requestedValue, string? currentValue)
{
    var resolvedValue = string.IsNullOrWhiteSpace(requestedValue) ? currentValue : requestedValue;
    resolvedValue = string.IsNullOrWhiteSpace(resolvedValue) ? DefaultRoundingRule : resolvedValue.Trim();
    var matchedRule = AllowedRoundingRules.FirstOrDefault(rule =>
        rule.Equals(resolvedValue, StringComparison.OrdinalIgnoreCase));

    if (matchedRule == null)
    {
        throw new ArgumentException("Rounding rule must be Floor, Ceil, or Round.");
    }

    return matchedRule;
}

private static decimal? ValidateAmount(decimal? value, string fieldName)
{
    if (value.HasValue && value.Value < 0)
    {
        throw new ArgumentException($"{fieldName} cannot be negative.");
    }

    return value;
}

private static decimal? ValidatePercentage(decimal? value, string fieldName)
{
    if (value.HasValue && (value.Value < 0 || value.Value > 100))
    {
        throw new ArgumentException($"{fieldName} must be between 0 and 100.");
    }

    return value;
}

private static string NormalizePayrollTrigger(string? trigger)
{
    if (string.IsNullOrWhiteSpace(trigger))
    {
        return "Manual";
    }

    return trigger.Trim().Equals("Manual", StringComparison.OrdinalIgnoreCase)
        ? "Manual"
        : "Scheduled";
}

        public async Task<PayrollRunResponse> RecalculateRunAsync(int runId)
        {
            var run = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.Id == runId);
            if (run == null) return null!;

            // Reset status to allow Hangfire to pick it up again
            run.Status = "Pending";
            run.IsRecalculated = true;
            run.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            // Re-queue the job via Hangfire
            BackgroundJob.Enqueue<PmHrmsAPI.PmHrmsBAL.Jobs.PayrollJob>(job => job.RunPayrollAsync(runId));

            // Return the updated run details
            return await GetPayrollRunAsync(runId);
        }
    }
}
