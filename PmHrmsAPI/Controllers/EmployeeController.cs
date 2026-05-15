using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models; 

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeBAL _employeeBAL;

        public EmployeeController(IEmployeeBAL employeeBAL)
        {
            _employeeBAL = employeeBAL;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var pagedData = await _employeeBAL.GetAllEmployees(pageNumber, pageSize, searchTerm);

                return Ok(new ApiResponseModel<PagedResult<EmployeeResponseModel>>(true, "Success", pagedData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error fetching employees", ex.Message));
            }
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var employee = await _employeeBAL.GetEmployee(id);

                if (employee == null)
                    return NotFound(new ApiResponseModel<string>(false, "Employee not found", null));

                return Ok(new ApiResponseModel<EmployeeResponseModel>(true, "Success", employee));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error", ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] EmployeeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseModel<object>(
                    false,
                    "Validation failed",
                    ModelState));

            try
            {
                var createdEmployee = await _employeeBAL.AddEmployee(model);

                return Ok(new ApiResponseModel<EmployeeResponseModel>(
                    true,
                    "Employee added successfully",
                    createdEmployee));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(
                    false,
                    ex.Message,
                    null));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new ApiResponseModel<string>(
                        false,
                        "Error adding employee",
                        ex.Message));
            }
        }
        
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(false);

            var exists = await _employeeBAL.EmailExists(email.Trim());
            return Ok(exists);
        }

        [HttpGet("check-code")]
        public async Task<IActionResult> CheckEmployeeCode([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(false);

            var orgIdClaim = User.FindFirst("OrgId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim))
                return Unauthorized();

            var orgId = int.Parse(orgIdClaim);

            var exists = await _employeeBAL.EmployeeCodeExists(code.Trim(), orgId);
            return Ok(exists);
        }
        [HttpGet("team-members/{employeeId}")]
        public async Task<IActionResult> GetTeamMembers(int employeeId)
        {
            var result = await _employeeBAL.GetTeamMembers(employeeId);

            return Ok(new ApiResponseModel<List<EmployeeResponseModel>>(
                true,
                "Success",
                result
            ));
        }

        [HttpPut("update-admin-profile")]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] AdminProfileUpdateModel model)
        {
            
            try
            {
                
                var updated = await _employeeBAL.UpdateAdminProfile(model);
                if (updated == null) return NotFound(new ApiResponseModel<string>(false, "Admin not found", null));

                return Ok(new ApiResponseModel<EmployeeResponseModel>(true, "Profile updated and activated", updated));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error updating profile", ex.Message));
            }
        }
        [HttpGet("{employeeId:int}/office-location")]
        public async Task<IActionResult> GetEmployeeOffice(int employeeId)
        {
            var location = await _employeeBAL.GetEmployeeOfficeLocation(employeeId);

            return Ok(new ApiResponseModel<object>(
                true,
                "Success",
                location
            ));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var orgId = int.Parse(User.FindFirst("OrgId")!.Value);

            var roles = await _employeeBAL.GetRoles(orgId);

            return Ok(new ApiResponseModel<RoleBundleResponse>(true, "Success", roles));
        }


        [HttpPut("{id}/profile-image")]
        public async Task<IActionResult> UpdateProfileImage(int id, IFormFile file)
        {
            if (file == null)
                return BadRequest("File required");

            var updated = await _employeeBAL.UpdateEmployeeProfileImage(id, file);

            if (updated == null)
                return NotFound();

            return Ok(new ApiResponseModel<string>(true, "Image updated", updated));
        }

        [HttpDelete("{id}/profile-image")]
        public async Task<IActionResult> DeleteProfileImage(int id)
        {
            var result = await _employeeBAL.DeleteEmployeeProfileImage(id);

            return Ok(new ApiResponseModel<bool>(true, "Deleted", result));
        }

        // Dedicated Endpoint for Official Information
        [HttpPut("{id}/official-info")]
        public async Task<IActionResult> UpdateOfficialInfo(int id, [FromBody] EmployeeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseModel<object>(false, "Validation Failed", ModelState));

            try
            {
                var updatedEmployee = await _employeeBAL.UpdateEmployee(id, model);

                if (updatedEmployee == null)
                    return NotFound(new ApiResponseModel<string>(false, "Employee not found", null));

                return Ok(new ApiResponseModel<EmployeeResponseModel>(true, "Official Information updated successfully", updatedEmployee));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // prints stack trace
                return StatusCode(500, new ApiResponseModel<string>(false, "Error updating official info", ex.Message));
            }
        }

        [HttpGet("check-phone")]
        public async Task<IActionResult> CheckPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(false);

            var exists = await _employeeBAL.PhoneExists(phone);
            return Ok(exists);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isDeleted = await _employeeBAL.DeleteEmployee(id);

                if (!isDeleted)
                    return NotFound(new ApiResponseModel<string>(false, "Employee not found", null));

                return Ok(new ApiResponseModel<string>(true, "Employee deactivated successfully", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error deleting employee", ex.Message));
            }
        }
    }
}