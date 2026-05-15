using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]                
    
    public class MigrationController : ControllerBase
    {
        private readonly IMigrationService _migrationService;                   
        private readonly ILogger<MigrationController> _logger;
        private readonly IPermissionService _permissionService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MigrationController(
          IMigrationService migrationService,
          IPermissionService permissionService,
          ILogger<MigrationController> logger , IServiceScopeFactory serviceScopeFactory)
        {
            _migrationService = migrationService;
            _permissionService = permissionService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

        }



        [HttpGet("active")]
        public async Task<IActionResult> GetActiveJob([FromQuery] string entityType)
        {
            _logger.LogInformation("GetActiveJob called for entityType: {EntityType}", entityType);

            _permissionService.Ensure(PermissionKeys.MIGRATION_VIEW);
            int orgId = int.Parse(User.FindFirst("OrgId")?.Value ?? "0");
            if (orgId == 0) return Unauthorized();

            try
            {
                var activeJob = await _migrationService.GetActiveJobAsync(orgId, entityType);

                if (activeJob == null)
                {
                    return Ok(new ApiResponseModel<object>(true, null, new { hasActive = false }));
                }

                var response = new
                {
                    hasActive = true,
                    id = activeJob.Id,
                    status = activeJob.Status,
                    progress = activeJob.TotalRecords > 0
                        ? (double)activeJob.ImportedCount / activeJob.TotalRecords * 100
                        : 0
                };

                return Ok(new ApiResponseModel<object>(true, null, response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveJob");
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }

        [HttpPost("auto-map")]
        public IActionResult GetAutoMapping([FromBody] AutoMappingRequest request)
        {
            _logger.LogInformation("GetAutoMapping called.");
            _permissionService.Ensure(PermissionKeys.MIGRATION_VIEW);

            if (request == null || request.SampleData == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = _migrationService.AutoMapWithConfidence(
                    request.ExcelColumns,
                    request.SystemFields,
                    request.SampleData
                );

                return Ok(new ApiResponseModel<object>(true, null, result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetAutoMapping.");
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }

        [HttpGet("fields/{entityType}")]
        public async Task<IActionResult> GetFields(string entityType)
        {
            _logger.LogInformation("GetFields called for entityType: {EntityType}.", entityType);
            _permissionService.Ensure(PermissionKeys.MIGRATION_VIEW);
            try
            {
                var fields = await _migrationService.GetConfigsByEntityAsync(entityType);
                _logger.LogInformation("Retrieved {FieldsCount} fields for entityType: {EntityType}.", fields?.Count ?? 0, entityType);
                return Ok(new ApiResponseModel<object>(true, null, fields));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetFields for entityType: {EntityType}.", entityType);
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }

        [HttpGet("template/{entityType}")]
        public async Task<IActionResult> DownloadTemplate(string entityType)
        {
            _logger.LogInformation("DownloadTemplate called for entityType: {EntityType}.", entityType);
            _permissionService.Ensure(PermissionKeys.MIGRATION_VIEW);
            try
            {
                var fields = await _migrationService.GetConfigsByEntityAsync(entityType);
                _logger.LogInformation("Retrieved {FieldsCount} fields for template generation.", fields?.Count ?? 0);
                var headers = fields.Select(f => f.Label).ToList();
                var fileBytes = _migrationService.GenerateExcelTemplate(headers);
                _logger.LogInformation("Generated Excel template for entityType: {EntityType}.", entityType);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{entityType}_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DownloadTemplate for entityType: {EntityType}.", entityType);
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }


        [HttpPost("validate")]
       
        public async Task<IActionResult> ValidateFullFile([FromBody] ImportRequestModel request)
        {
            _logger.LogInformation("ValidateFullFile called with {RowCount} rows", request?.Rows?.Count ?? 0);
            _permissionService.Ensure(PermissionKeys.DATA_MIGRATE);


            if (request == null || !request.Rows.Any())
            {
                return BadRequest(new ApiResponseModel<object>(false, "No data provided.", null));
            }

            int orgId = int.Parse(User.FindFirst("OrgId")?.Value ?? "0");
            if (orgId == 0) return Unauthorized();

            try
            {
                var validationResult = await _migrationService.ValidateFullFileAsync(request, orgId);
                return Ok(new ApiResponseModel<object>(true, null, validationResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateFullFile");
                return StatusCode(500, new ApiResponseModel<object>(false, $"Validation failed: {ex.Message}", null));
            }
        }

        [HttpPost("import")]
        
        public async Task<IActionResult> StartImport([FromBody] ImportRequestModel request)
        {
            _logger.LogInformation("StartImport called.");
            _permissionService.Ensure(PermissionKeys.DATA_MIGRATE);
            if (request == null || !request.Rows.Any())
            {
                _logger.LogWarning("No data provided for import in StartImport.");
                return BadRequest(new ApiResponseModel<object>(false, "No data provided for import.", null));
            }
            int orgId = int.Parse(User.FindFirst("OrgId")?.Value ?? "1");
            int userId = int.Parse(User.FindFirst("EmployeeId")?.Value ?? "1");
            _logger.LogInformation("Starting import for entityType: {EntityType}, rows count: {RowsCount}, orgId: {OrgId}, userId: {UserId}.",
                request.EntityType, request.Rows.Count, orgId, userId);
            try
            {

                var existingJob = await _migrationService.GetActiveJobAsync(orgId, request.EntityType);
                if (existingJob != null)
                {
                    return BadRequest(new ApiResponseModel<object>(false, "An import is already in progress.", null));
                }


                var jobId = await _migrationService.CreateJobAsync(orgId, userId, request.EntityType, request.Rows.Count, request.FileName);
                _logger.LogInformation("Job created with jobId: {JobId}.", jobId);
                BackgroundJob.Enqueue<IMigrationService>(service =>
                service.ProcessImportAsync(jobId, request, orgId));


                return Ok(new ApiResponseModel<Guid>(true, null, jobId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in StartImport.");
                return StatusCode(500, new ApiResponseModel<object>(false, $"Failed to queue import: {ex.Message}", null));
            }
        }

        [HttpGet("import/{jobId}/status")]
        public async Task<IActionResult> GetImportStatus(Guid jobId)
        {
            _logger.LogInformation("GetImportStatus called for jobId: {JobId}.", jobId);
            var orgIdClaim = User.FindFirst("OrgId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim))
            {
                _logger.LogWarning("Unauthorized access attempt in GetImportStatus.");
                return Unauthorized();
            }
            int currentOrgId = int.Parse(orgIdClaim);
            try
            {
                var job = await _migrationService.GetJobByIdAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning("Job not found for jobId: {JobId}.", jobId);
                    return NotFound(new ApiResponseModel<object>(false, "Job not found", null));
                }
                if (job.OrgId != currentOrgId)
                {
                    _logger.LogWarning("Forbidden access for jobId: {JobId}, orgId mismatch: requested {CurrentOrgId}, job OrgId {JobOrgId}.", jobId, currentOrgId, job.OrgId);
                    return Forbid();
                }
                var statusData = new
                {
                    jobId = job.Id,
                    status = job.Status,
                    total = job.TotalRecords,
                    validated = job.ValidatedCount,
                    imported = job.ImportedCount,
                    failed = job.FailedCount,
                    currentStep = job.CurrentStep,
                    canCancel = IsJobCancellable(job.Status),
                    errorLog = IsJobFinished(job.Status) ? job.ErrorLog : null,
                    sharedPassword = job.Status == "COMPLETED" ? job.SharedPassword : null
                };
                _logger.LogInformation("Retrieved status for jobId: {JobId}, status: {Status}.", jobId, job.Status);
                return Ok(new ApiResponseModel<object>(true, null, statusData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetImportStatus for jobId: {JobId}.", jobId);
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }




       

        [HttpPost("import/{jobId}/stop")]
        
        public async Task<IActionResult> StopImport(Guid jobId)
        {
            _logger.LogInformation("StopImport called for jobId: {JobId}.", jobId);
            _permissionService.Ensure(PermissionKeys.DATA_MIGRATE);
            try
            {
                var result = await _migrationService.CancelJobAsync(jobId);
                if (!result.Success)
                {
                    _logger.LogWarning("CancelJobAsync failed for jobId: {JobId}, message: {Message}.", jobId, result.Message);
                    return BadRequest(new ApiResponseModel<object>(false, result.Message, null));
                }
                _logger.LogInformation("Job cancelled successfully for jobId: {JobId}.", jobId);
                return Ok(new ApiResponseModel<object>(true, result.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in StopImport for jobId: {JobId}.", jobId);
                return StatusCode(500, new ApiResponseModel<object>(false, $"Internal server error: {ex.Message}", null));
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string entityType)
        {
            _permissionService.Ensure(PermissionKeys.MIGRATION_VIEW);

            int orgId = int.Parse(User.FindFirst("OrgId")?.Value ?? "0");
            if (orgId == 0) return Unauthorized();

            try
            {
                var history = await _migrationService.GetHistoryAsync(orgId, entityType);

                var result = history.Select(j => new
                {
                    jobId = j.Id,
                    entityType = j.EntityType,
                    status = j.Status,
                    total = j.TotalRecords,
                    imported = j.ImportedCount,
                    failed = j.FailedCount,
                    createdAt = j.CreatedAt,
                    completedAt = j.CompletedAt
                });

                return Ok(new ApiResponseModel<object>(true, null, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<object>(false, ex.Message, null));
            }
        }



        private bool IsJobCancellable(string status)
        {
            return status is "QUEUED" or "SCANNING" or "PREPARING_MASTERS" or "VALIDATING" or "IMPORTING";
        }

        private bool IsJobFinished(string status)
        {
            return status is "COMPLETED" or "PARTIAL_SUCCESS" or "FAILED" or "CANCELLED";
        }

    }
}