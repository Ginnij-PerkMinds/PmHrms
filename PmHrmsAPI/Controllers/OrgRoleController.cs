using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    // Controllers/OrgRoleController.cs
    [Route("api/[controller]")]
    [ApiController]
    public class OrgRoleController : ControllerBase
    {
        private readonly IOrgRoleBAL _roleBal;

        public OrgRoleController(IOrgRoleBAL roleBal)
        {
            _roleBal = roleBal;
        }

        // Moved from Employee Controller
        [HttpGet("bundle")]
        public async Task<IActionResult> GetRoles()
        {
            if (!TryGetClaimOrgId(out var orgId, out var unauthorized))
                return unauthorized!;

            var roles = await _roleBal.GetRoles(orgId);
            return Ok(new ApiResponseModel<RoleBundleResponse>(true, "Success", roles));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OrgRoleRequest request)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;

            if (request.OrgId != 0 && request.OrgId != claimOrgId)
                return Forbid();

            var response = await _roleBal.CreateRole(request.Name, claimOrgId);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] string name)
        {
            var response = await _roleBal.UpdateRole(id, name);
            if (!response.Success) return NotFound(response);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _roleBal.DeleteRole(id);
            if (isDeleted) return Ok(new ApiResponseModel<bool>(true, "Role deleted", true));
            return NotFound(new ApiResponseModel<bool>(false, "Role not found", false));
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

   
