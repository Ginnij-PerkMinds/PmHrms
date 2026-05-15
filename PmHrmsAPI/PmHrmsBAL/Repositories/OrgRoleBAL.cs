using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class OrgRoleBAL : IOrgRoleBAL
    {
        private readonly OrgRoleDAL _roleDal;
        private readonly IPermissionService _permissionService;

        public OrgRoleBAL(OrgRoleDAL roleDal, IPermissionService permissionService)
        {
            _roleDal = roleDal;
            _permissionService = permissionService;
        }

        public async Task<RoleBundleResponse> GetRoles(int orgId)
        {
            _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
            var systemRoles = await _roleDal.GetSystemRoles();
            var orgRoles = await _roleDal.GetOrgRoles(orgId);

            return new RoleBundleResponse
            {
                SystemRoles = systemRoles.Select(r => new RoleResponseModel { RoleId = r.SystemRoleId, RoleName = r.Name }).ToList(),
                OrgRoles = orgRoles.Select(r => new RoleResponseModel { RoleId = r.OrgRoleId, RoleName = r.Name }).ToList()
            };
        }

        public async Task<ApiResponseModel<OrgRole>> CreateRole(string name, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
            var role = new OrgRole { Name = name, OrgId = orgId };
            var result = await _roleDal.AddOrgRole(role);
            //return new ApiResponseModel<OrgRole>(true, "Role created", result);
            return new ApiResponseModel<OrgRole>(true, PmHrmsConstants.OrgRoleMessages.RoleCreated, result);
        }

        public async Task<ApiResponseModel<OrgRole>> UpdateRole(int id, string name)
        {
            _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
            var existing = await _roleDal.GetOrgRoleById(id);
            if (existing == null)
                //return new ApiResponseModel<OrgRole>(false, "Role not found", null);
                return new ApiResponseModel<OrgRole>(false, PmHrmsConstants.OrgRoleMessages.RoleNotFound, null);

            existing.Name = name;
            var result = await _roleDal.UpdateOrgRole(existing);
            //return new ApiResponseModel<OrgRole>(true, "Role updated", result);
            return new ApiResponseModel<OrgRole>(true, PmHrmsConstants.OrgRoleMessages.RoleUpdated, result);
        }

        public async Task<bool> DeleteRole(int id)
        {
            _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
            return await _roleDal.DeleteOrgRole(id);
        }
    }
}