using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkPolicyController : ControllerBase    
    {
        private readonly IWorkPolicyBAL _bal;

        [HttpPost("assign-to-designation")]
        public async Task<IActionResult> AssignToDesignation(int designationId, int policyId)
        {
            await _bal.AssignPolicyToDesignation(designationId, policyId);
            return Ok(new ApiResponseModel<object>(true, "Assigned", null));
        }

        public WorkPolicyController(IWorkPolicyBAL bal)                                                                                     
        {     
            _bal = bal;   
        }                                         

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
                {
            var pagedData = await _bal.GetAll(pageNumber, pageSize, searchTerm);

            return Ok(new ApiResponseModel<PagedResult<WorkPolicyResponseModel>>(
                true,
                "Success",
                pagedData));
        }

        [HttpGet("designation-mappings")]
        public async Task<IActionResult> GetDesignationMappings()
        {
            var result = await _bal.GetDesignationPolicyMappings();
            return Ok(new ApiResponseModel<object>(true, "Success", result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkPolicyModel model)
        {
            var result = await _bal.Create(model);
            return Ok(new ApiResponseModel<object>(true, "Created", result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkPolicyModel model)
        {
            var result = await _bal.Update(id, model);
            if (result == null)
                return NotFound();

            return Ok(new ApiResponseModel<object>(true, "Updated", result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _bal.GetById(id);

            if (result == null)
                return NotFound();

            return Ok(new ApiResponseModel<object>(true, "Success", result));
        }

        [HttpDelete("remove-designation")]
        public async Task<IActionResult> RemoveDesignation(int designationId, int policyId)
        {
            await _bal.RemovePolicyFromDesignation(designationId, policyId);
            return Ok(new ApiResponseModel<object>(true, "Removed", null));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bal.Delete(id);
            return Ok(new ApiResponseModel<bool>(true, "Deleted", deleted));
        }
    }
}
