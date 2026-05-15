using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL.Repositories;

public class RolePermissionBAL : IRolePermissionBAL
{
    private readonly RolePermissionDAL _dal;
    private readonly IPermissionService _permissionService;

    public RolePermissionBAL(RolePermissionDAL dal, IPermissionService permissionService)
    {
        _dal = dal;
        _permissionService = permissionService;
    }

    public async Task<List<PermissionMaster>> FetchAllMasterPermissions()
    {
        _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);
        return await _dal.GetAllPermissions();
    }

    public async Task<List<int>> GetAssignedPermissionIds(int orgRoleId)
    {
        _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);
        var mappings = await _dal.GetMappingsByRole(orgRoleId);
        return mappings.Select(m => m.PermissionId).ToList();
    }

    public async Task<List<string>> GetAssignedPermissionKeys(int orgRoleId)
    {
      //  _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);
        var mappings = await _dal.GetMappingsByRole(orgRoleId);
        return mappings
            .Where(m => m.IsActive == true && m.Permission?.PermissionKey != null)
             .Select(m => m.Permission!.PermissionKey!)
               .ToList();
    }

    public async Task<bool> UpdatePermissionsForRole(int roleId, List<int> pIds)
    {
        _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);
        //if (roleId <= 0) return false;
        if (roleId <= PmHrmsConstants.RolePermissionMessages.InvalidId) return false;

        var newMappings = pIds.Select(pId => new RolePermission
        {
            OrgRoleId = roleId,
            PermissionId = pId,
            IsActive = true,
            CreatedAt = DateTime.Now
        }).ToList();

        return await _dal.TransactionalSync(roleId, newMappings);
    }

    public async Task<List<UserRoleAssignmentModel>> GetAssignableUsers(int orgId)
    {
        _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);
        return await _dal.GetUsersWithRoles(orgId);
    }

    public async Task<bool> AssignRoleToUser(int orgId, int employeeId, int orgRoleId)
    {
        _permissionService.Ensure(PermissionKeys.PERM_ASSIGN);

        //if (employeeId <= 0 || orgRoleId <= 0)
        if (employeeId <= PmHrmsConstants.RolePermissionMessages.InvalidId || orgRoleId <= PmHrmsConstants.RolePermissionMessages.InvalidId)
        {
            return false;
        }

        return await _dal.AssignRoleToUser(orgId, employeeId, orgRoleId);
    }
}
