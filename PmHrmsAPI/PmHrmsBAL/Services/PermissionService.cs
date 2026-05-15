using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.Exceptions;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using System.Security.Claims;


public class PermissionService : IPermissionService
{
    private readonly PmHrmsContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private HashSet<string>? _cachedPermissions;

    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        PmHrmsContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private async Task<HashSet<string>> LoadPermissionsAsync()
    {
        if (_cachedPermissions != null)
        {
            _logger.LogInformation("Using cached permissions");
            return _cachedPermissions;
        }

        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
        {
            _logger.LogWarning("User not authenticated");
            return _cachedPermissions = new HashSet<string>();
        }

        _logger.LogInformation("---- USER CLAIMS ----");
        foreach (var claim in user.Claims)
        {
            _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }

        var orgRoleIdClaim = user.Claims
            .FirstOrDefault(c =>
                c.Type.Equals("OrgRoleId", StringComparison.OrdinalIgnoreCase))
            ?.Value;

        _logger.LogInformation("OrgRoleId claim found: {OrgRoleId}", orgRoleIdClaim);

        if (!int.TryParse(orgRoleIdClaim, out int orgRoleId))
        {
            _logger.LogError("Failed to parse OrgRoleId claim");
            return _cachedPermissions = new HashSet<string>();
        }

        _logger.LogInformation("Parsed OrgRoleId: {OrgRoleId}", orgRoleId);

        var permissions = await (
            from rp in _context.RolePermissions
            join pm in _context.PermissionMasters
                on rp.PermissionId equals pm.PermissionId
            where rp.OrgRoleId == orgRoleId && rp.IsActive == true
            select pm.PermissionKey
        ).ToListAsync();

        _logger.LogInformation("Permissions loaded from DB:");

        foreach (var p in permissions)
        {
            _logger.LogInformation("Permission: {Permission}", p);
        }

        _cachedPermissions = permissions.ToHashSet();

        return _cachedPermissions;
    }

    public bool Has(string permissionKey)
    {
        var permissions = LoadPermissionsAsync().GetAwaiter().GetResult();

        var result = permissions.Contains(permissionKey);

        _logger.LogInformation(
            "Checking permission: {PermissionKey} | Result: {Result}",
            permissionKey,
            result
        );

        return result;
    }
    public void Ensure(string permissionKey)
    {
        if (!Has(permissionKey))
            throw new ForbiddenException($"Permission denied: {permissionKey}");
    }



    public int GetCurrentEmployeeId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var empIdClaim = user?.Claims
            .FirstOrDefault(c => c.Type.Equals("EmployeeId", 
                StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (!int.TryParse(empIdClaim, out int empId))
        {
            _logger.LogWarning("[PermissionService] EmployeeId claim missing or invalid");
            //throw new UnauthorizedException("Employee identity could not be resolved.");
        }

        return empId;
    }

    public bool IsSelf(int targetEmployeeId)
    {
        return GetCurrentEmployeeId() == targetEmployeeId;
    }


        public void EnsureNotSelf(int targetEmployeeId, string action)
    {
        if (IsSelf(targetEmployeeId))
        {
            _logger.LogWarning(
                "[PermissionService] Self-action blocked | Action: {Action} | EmpId: {EmpId}",
                action, targetEmployeeId);

            throw new ForbiddenException($"You cannot {action} for yourself.");
        }
    }

     public void EnsureCanActOn(string permissionKey, int targetEmployeeId)
    {
        Ensure(permissionKey);             
        EnsureNotSelf(targetEmployeeId,     
            $"perform '{permissionKey}'");
    }

    //public void EnsureCanActOn(string aTT_EDIT_LOGS, object EmployeeId)
    //{
    //    throw new NotImplementedException();
    //}


}

