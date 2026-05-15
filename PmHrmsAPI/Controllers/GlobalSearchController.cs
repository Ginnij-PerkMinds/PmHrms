using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using System.Diagnostics;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly IGlobalSearchBAL _globalSearchBAL;
        private readonly ILogger<GlobalSearchController> _logger;

        public GlobalSearchController(
            IGlobalSearchBAL globalSearchBAL,
            ILogger<GlobalSearchController> logger)
        {
            _globalSearchBAL = globalSearchBAL;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? searchTerm,
            [FromQuery] int limit = 5,
            [FromQuery] string scope = "all")
        {
            var sw = Stopwatch.StartNew();
            var userIdClaim = User.FindFirst("EmployeeId")?.Value;
            _logger.LogInformation(
                "GlobalSearch request received. EmployeeId: {EmployeeId}, RawLimit: {Limit}, Scope: {Scope}, HasSearchTerm: {HasSearchTerm}",
                userIdClaim,
                limit,
                scope,
                !string.IsNullOrWhiteSpace(searchTerm));

            if (!TryGetClaimOrgId(out var orgId, out var unauthorized))
            {
                _logger.LogWarning(
                    "GlobalSearch unauthorized request. EmployeeId: {EmployeeId}, Reason: invalid OrgId claim",
                    userIdClaim);
                return unauthorized!;
            }

            var result = await _globalSearchBAL.Search(searchTerm ?? string.Empty, orgId, limit, scope);

            sw.Stop();
            _logger.LogInformation(
                "GlobalSearch completed. EmployeeId: {EmployeeId}, OrgId: {OrgId}, Total: {Total}, Employees: {Employees}, Departments: {Departments}, Designations: {Designations}, Documents: {Documents}, DurationMs: {DurationMs}",
                userIdClaim,
                orgId,
                result.Total,
                result.Employees.Count,
                result.Departments.Count,
                result.Designations.Count,
                result.Documents.Count,
                sw.ElapsedMilliseconds);

            return Ok(new ApiResponseModel<GlobalSearchResponseModel>(true, "Success", result));
        }

        private bool TryGetClaimOrgId(out int orgId, out IActionResult? unauthorized)
        {
            unauthorized = null;
            orgId = 0;

            var orgClaim = User.FindFirst("OrgId")?.Value;
            if (!int.TryParse(orgClaim, out orgId) || orgId <= 0)
            {
                unauthorized = Unauthorized(new ApiResponseModel<string>(false, "Invalid token claims", string.Empty));
                return false;
            }

            return true;
        }
    }
}
