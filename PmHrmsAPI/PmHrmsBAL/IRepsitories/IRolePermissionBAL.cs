using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories;

public interface IRolePermissionBAL
{
    Task<List<PermissionMaster>> FetchAllMasterPermissions();
    Task<List<int>> GetAssignedPermissionIds(int orgRoleId);
    Task<List<string>> GetAssignedPermissionKeys(int orgRoleId);
    Task<bool> UpdatePermissionsForRole(int roleId, List<int> pIds);
    Task<List<UserRoleAssignmentModel>> GetAssignableUsers(int orgId);
    Task<bool> AssignRoleToUser(int orgId, int employeeId, int orgRoleId);
}
