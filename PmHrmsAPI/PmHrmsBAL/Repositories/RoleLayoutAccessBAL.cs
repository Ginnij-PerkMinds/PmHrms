using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class RoleLayoutAccessBAL : IRoleLayoutAccessBAL
    {
        private readonly RoleLayoutAccessDAL _dal;
        private readonly IPermissionService _permissionService;

        public RoleLayoutAccessBAL(RoleLayoutAccessDAL dal, IPermissionService permissionService)
        {
            _dal = dal;
            _permissionService = permissionService;
        }

        //public async Task<DefaultLayoutResponse> GetDefaultLayoutForRole(int systemRoleId)
        //{
        //    _permissionService.Ensure(PermissionKeys.DASHBOARD_ADMIN_VIEW);
        //    var layoutKey = await _dal.GetDefaultLayoutKey(systemRoleId);

        //    return new DefaultLayoutResponse
        //    {
        //        SystemRoleId = systemRoleId,
        //        LayoutKey = layoutKey ?? "EMPLOYEE"
        //    };
        //}

        public async Task<DefaultLayoutResponse> GetDefaultLayoutForRole(int systemRoleId)
        {
            var layoutKey = await _dal.GetDefaultLayoutKey(systemRoleId);

            return new DefaultLayoutResponse
            {
                SystemRoleId = systemRoleId,
                //LayoutKey = layoutKey ?? "EMPLOYEE"
                LayoutKey = layoutKey ?? PmHrmsConstants.RoleLayoutMessages.EmployeeLayoutKey
            };
        }

        public async Task<List<RoleLayoutAccessResponse>> GetRoleLayoutAccess(int systemRoleId)
        {
            _permissionService.Ensure(PermissionKeys.DASHBOARD_ADMIN_VIEW);
            var entities = await _dal.GetRoleLayoutAccess(systemRoleId);

            return entities.Select(x => new RoleLayoutAccessResponse
            {
                SystemRoleId = x.SystemRoleId,
                RoleName = x.SystemRole.Name,
                LayoutId = x.LayoutId,
                LayoutKey = x.Layout.LayoutKey,
                LayoutName = x.Layout.LayoutName,
                IsAllowed = x.IsAllowed
            }).ToList();
        }

        public async Task<RoleLayoutAccessResponse> AssignLayout(RoleLayoutAccessRequest request)
        {
            _permissionService.Ensure(PermissionKeys.DASHBOARD_ADMIN_VIEW);
            var entity = new RoleLayoutAccess
            {
                SystemRoleId = request.SystemRoleId,
                LayoutId = request.LayoutId,
                IsAllowed = request.IsAllowed
            };

            var result = await _dal.Upsert(entity);

            return new RoleLayoutAccessResponse
            {
                SystemRoleId = result.SystemRoleId,
                LayoutId = result.LayoutId,
                IsAllowed = result.IsAllowed
            };
        }
    }
}