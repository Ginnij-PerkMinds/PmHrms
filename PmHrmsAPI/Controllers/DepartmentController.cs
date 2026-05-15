using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentBAL _departmentBAL;

        public DepartmentController(IDepartmentBAL departmentBAL)
        {
            _departmentBAL = departmentBAL;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {

                var pagedData = await _departmentBAL.GetAllDepartments(pageNumber, pageSize, searchTerm);

                return Ok(new ApiResponseModel<PagedResult<DepartmentResponseModel>>(true, "Success", pagedData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "Internal server error", ex.Message));
            }
        }

       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _departmentBAL.GetDepartment(id);

                if (result == null)
                    return NotFound(new ApiResponseModel<string>(
                        false, "Department not found", null));

                return Ok(new ApiResponseModel<DepartmentResponseModel>(
                    true, "Success", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "Internal server error", ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] DepartmentModel model)
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

                var result = await _departmentBAL.AddDepartment(model, orgId, empId);

                return Ok(new ApiResponseModel<DepartmentResponseModel>(
                    true,
                    "Department added successfully",
                    result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseModel<string>(
                    false,
                    ex.Message,
                    null));
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentModel model)
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
                var result = await _departmentBAL.UpdateDepartment(id, model);

                if (result == null)
                    return NotFound(new ApiResponseModel<string>(
                        false, "Department not found", null));

                return Ok(new ApiResponseModel<DepartmentResponseModel>(
                    true, "Department updated successfully", result));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new ApiResponseModel<string>(
                    false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "Internal server error", ex.Message));
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _departmentBAL.DeleteDepartment(id);

                if (!result)
                    return NotFound(new ApiResponseModel<string>(
                        false, "Department not found", null));

                return Ok(new ApiResponseModel<string>(
                    true, "Department deleted successfully", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "Internal server error", ex.Message));
            }
        }
    }
}
