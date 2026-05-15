using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;


namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationController : ControllerBase
    {
        private readonly IDesignationBAL _designationBAL;

        public DesignationController(IDesignationBAL designationBAL)
        {
            _designationBAL = designationBAL;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var orgClaim = User.FindFirst("OrgId");

                if (orgClaim == null)
                {
                    return Unauthorized("OrgId claim missing in token");
                }

                int orgId = int.Parse(orgClaim.Value);
                var pagedData = await _designationBAL.GetAllDesignations(pageNumber, pageSize, searchTerm, orgId);

                return Ok(new ApiResponseModel<PagedResult<DesignationResponseModel>>(true, "Success", pagedData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error fetching designations", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int orgId = int.Parse(User.FindFirst("OrgId")!.Value);
            var result = await _designationBAL.GetDesignation(id , orgId);
           
            if (result == null) 
                return NotFound(new ApiResponseModel<string>(false, "Not Found", null));
            return 
                Ok(
                    new ApiResponseModel<DesignationResponseModel>(true, "Success", result)
                    );
        }



        [HttpGet("by-department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            int orgId = int.Parse(User.FindFirst("OrgId")!.Value);

            var data = await _designationBAL.GetDesignationsByDepartment(departmentId, orgId);

            return Ok(new ApiResponseModel<List<DesignationResponseModel>>(
                true, "Success", data
            ));
        }


        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DesignationModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => x.Value.Errors.First().ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponseModel<List<string>>(
                    false, "Validation failed", errors));
            }

            try
            {
                var orgClaim = User.FindFirst("OrgId");
                var empClaim = User.FindFirst("EmployeeId");

                if (orgClaim == null || empClaim == null)
                    return Unauthorized(new ApiResponseModel<string>(
                        false, "Invalid token claims", null));

                int orgId = int.Parse(orgClaim.Value);
                int empId = int.Parse(empClaim.Value);

                var result =
                    await _designationBAL.AddDesignation(model, orgId, empId);

                return Ok(new ApiResponseModel<DesignationResponseModel>(
                    true,
                    "Designation added successfully",
                    result));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new ApiResponseModel<string>(
                    false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new ApiResponseModel<string>(
                        false, "Internal server error", ex.Message));
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DesignationModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int orgId = int.Parse(User.FindFirst("OrgId")!.Value);

            var
                result = await _designationBAL.UpdateDesignation(id, model , orgId);
            if (result == null)
                return NotFound(new ApiResponseModel<string>(false, "Not Found", null));
            return 
                Ok(
                    new ApiResponseModel<DesignationResponseModel>(true, "Updated", result)
                    );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = 
                await _designationBAL.DeleteDesignation(id);
            if (!result) 
                return NotFound(new ApiResponseModel<string>(false, "Not Found", null));
            return
                Ok(
                    new ApiResponseModel<string>(true, "Deleted", null)
                    );
        }
    }
}