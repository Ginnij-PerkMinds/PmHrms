using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetOrgId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("OrgId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public int GetCurrentUserID()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("EmployeeId");
            return claim != null ? int.Parse(claim.Value): 0;
        }

        public int? GetDepartmentId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("DepartmentId");
            return claim != null ? int.Parse(claim.Value) : null;
        }
        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
