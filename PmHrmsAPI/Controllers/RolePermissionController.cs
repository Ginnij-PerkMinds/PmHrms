using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionBAL _bal;

        public RolePermissionController(IRolePermissionBAL bal)
        {
            _bal = bal;
        }

        [HttpGet("master-list")]
        public async Task<IActionResult> GetMasterList()
        {
            var data = await _bal.FetchAllMasterPermissions();
            return Ok(new ApiResponseModel<List<PermissionMaster>>(true, "Success", data));
        }

        // Yeh Angular mein checkboxes tick karne ke liye use hoga
        [HttpGet("role-list/{id}")]
        public async Task<IActionResult> GetRolePermissions(int id)
        {
            var permissionIds = await _bal.GetAssignedPermissionIds(id);
            return Ok(new ApiResponseModel<List<int>>(true, "Success", permissionIds));
        }

        [HttpGet("role-keys/{id}")]
        public async Task<IActionResult> GetRolePermissionKeys(int id)
        {
            var permissionKeys = await _bal.GetAssignedPermissionKeys(id);
            return Ok(new ApiResponseModel<List<string>>(true, "Success", permissionKeys));
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncMapping([FromBody] RolePermissionRequest req)
        {
            var result = await _bal.UpdatePermissionsForRole(req.OrgRoleId, req.PermissionIds);
            if (result)
                return Ok(new ApiResponseModel<bool>(true, "Permissions updated successfully", true));

            return BadRequest(new ApiResponseModel<bool>(false, "Failed to update permissions", false));
        }

        [HttpGet("user-role-list")]
        public async Task<IActionResult> GetUserRoleList()
        {
            if (!TryGetClaimOrgId(out var orgId, out var unauthorized))
            {
                return unauthorized!;
            }

            var users = await _bal.GetAssignableUsers(orgId);
            return Ok(new ApiResponseModel<List<UserRoleAssignmentModel>>(true, "Success", users));
        }

        [HttpPost("assign-user-role")]
        public async Task<IActionResult> AssignUserRole([FromBody] AssignUserRoleRequest request)
        {
            if (!TryGetClaimOrgId(out var orgId, out var unauthorized))
            {
                return unauthorized!;
            }

            var assigned = await _bal.AssignRoleToUser(orgId, request.EmployeeId, request.OrgRoleId);
            if (!assigned)
            {
                return BadRequest(new ApiResponseModel<bool>(false, "Failed to assign role to user", false));
            }

            return Ok(new ApiResponseModel<bool>(true, "User role updated successfully", true));
        }

        private bool TryGetClaimOrgId(out int orgId, out IActionResult? unauthorized)
        {
            unauthorized = null;
            orgId = 0;

            var orgClaim = User.FindFirst("OrgId")?.Value;
            if (!int.TryParse(orgClaim, out orgId) || orgId <= 0)
            {
                unauthorized = Unauthorized(new ApiResponseModel<string>(false, "Invalid token claims", null));
                return false;
            }

            return true;
        }


    }

}
