using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleLayoutAccessController : ControllerBase
    {
        private readonly IRoleLayoutAccessBAL _bal;        

        public RoleLayoutAccessController(IRoleLayoutAccessBAL bal)
        {
            _bal = bal;                                                                                                                                                                         
        }
      
        // 🔹 Used during login
        [HttpGet("default-layout/{systemRoleId}")]
        public async Task<IActionResult> GetDefaultLayout(int systemRoleId)
        {
            var result = await _bal.GetDefaultLayoutForRole(systemRoleId);
            return Ok(new ApiResponseModel<DefaultLayoutResponse>(true, "Default layout resolved", result));
        }

                                     
        [HttpGet("{systemRoleId}")]
        public async Task<IActionResult> GetRoleLayoutAccess(int systemRoleId)
        {
            var result = await _bal.GetRoleLayoutAccess(systemRoleId);
            return Ok(new ApiResponseModel<List<RoleLayoutAccessResponse>>(true, "Role layout access fetched", result));
        }
                  
         
        
        [HttpPost]
        public async Task<IActionResult> AssignLayout([FromBody] RoleLayoutAccessRequest request)
        {
            var result = await _bal.AssignLayout(request);           
            return Ok(new ApiResponseModel<RoleLayoutAccessResponse>(
                true,
                "Layout access updated",
                result
            ));
        }
    }
}
