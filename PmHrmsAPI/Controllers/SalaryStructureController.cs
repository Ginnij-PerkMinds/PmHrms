using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryStructureController : ControllerBase
    {
        private readonly ISalaryStructureBAL _bal;
        private readonly ILogger<SalaryStructureController> _logger;

        public SalaryStructureController(ISalaryStructureBAL bal, ILogger<SalaryStructureController> logger)
        {
            _bal = bal;
            _logger = logger;
        }

        // ✅ Get All
       [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation(
                    "[SalaryStructure GetAll] Request - Page: {Page}, Size: {Size}, Search: {Search}",
                    pageNumber, pageSize, search ?? "none");

                var data = await _bal.GetAll(pageNumber, pageSize, search);

                return Ok(new ApiResponseModel<PagedResult<SalaryStructureDTO>>(
                    true, "Success", data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalaryStructure GetAll] Error");
                return StatusCode(500, new ApiResponseModel<object>(
                    false, "Error retrieving salary structures", null));
            }
        }

        // ✅ Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalaryStructureModel model)
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure Create] Request received - StructureName: {model?.StructureName}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[SalaryStructure Create] ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors))}");
                    return BadRequest(ModelState);
                }

                var result = await _bal.Create(model);
                _logger.LogInformation($"[SalaryStructure Create] Success - Created ID: {result.SalaryStructureId}");

                return Ok(new ApiResponseModel<SalaryStructureDTO>(
                    true,
                    "Created",
                    result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure Create] Error - {ex.Message}\nInner Exception: {ex?.InnerException?.Message}\nStackTrace: {ex?.StackTrace}");
                return StatusCode(500, new ApiResponseModel<object>(
                    false,
                    $"Error creating salary structure: {ex.Message}",
                    null));
            }
        }

        // ✅ Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalaryStructureModel model)
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure Update] Request received - ID: {id}, StructureName: {model?.StructureName}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"[SalaryStructure Update] ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors))}");
                    return BadRequest(ModelState);
                }

                var result = await _bal.Update(id, model);

                if (result == null)
                {
                    _logger.LogWarning($"[SalaryStructure Update] Not found - ID: {id}");
                    return NotFound(new ApiResponseModel<object>(false, "Not Found", null));
                }

                _logger.LogInformation($"[SalaryStructure Update] Success - Updated ID: {id}");
                return Ok(new ApiResponseModel<SalaryStructureDTO>(
                    true,
                    "Updated",
                    result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure Update] Error - ID: {id}, {ex.Message}\nInner Exception: {ex?.InnerException?.Message}\nStackTrace: {ex?.StackTrace}");
                return StatusCode(500, new ApiResponseModel<object>(
                    false,
                    $"Error updating salary structure: {ex.Message}",
                    null));
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure Delete] Request received - ID: {id}");
                
                var deleted = await _bal.Delete(id);
                
                _logger.LogInformation($"[SalaryStructure Delete] Success - Deleted: {deleted}");
                return Ok(new ApiResponseModel<bool>(
                    true,
                    "Deleted",
                    deleted));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure Delete] Error - ID: {id}, {ex.Message}\\nInner Exception: {ex?.InnerException?.Message}\\nStackTrace: {ex?.StackTrace}");
                return StatusCode(500, new ApiResponseModel<object>(
                    false,
                    $"Error deleting salary structure: {ex.Message}",
                    null));
            }
        }

        // ✅ Assign to Designation
        [HttpPost("assign-to-designation")]
        public async Task<IActionResult> AssignToDesignation([FromBody] AssignDesignationModel model)
        {
            try
            {
                await _bal.AssignToDesignation(model.DesignationId, model.SalaryStructureId);

                return Ok(new ApiResponseModel<object>(
                    true, "Assigned", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalaryStructure AssignToDesignation] Error");
                return StatusCode(500, new ApiResponseModel<object>(
                    false, "Error assigning designation", null));
            }
        }

        // ✅ Get Designation Mappings
        [HttpGet("designation-mappings")]
        public async Task<IActionResult> GetMappings()
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure GetMappings] Request received");
                
                var data = await _bal.GetDesignationMappings();
                
                _logger.LogInformation($"[SalaryStructure GetMappings] Success - Returned {data.Count} mappings");
                return Ok(new ApiResponseModel<List<DesignationSalaryMappingDTO>>(
                    true,
                    "Success",
                    data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure GetMappings] Error - {ex.Message}\\nInner Exception: {ex?.InnerException?.Message}\\nStackTrace: {ex?.StackTrace}");
                return StatusCode(500, new ApiResponseModel<object>(
                    false,
                    $"Error retrieving mappings: {ex.Message}",
                    null));
            }
        }

        
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetSalaryByEmployee(int employeeId)
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure GetSalaryByEmployee] Request received - EmployeeId: {employeeId}");
                
                var result = await _bal.GetSalaryByEmployeeId(employeeId);

                if (result == null)
                {
                    _logger.LogWarning($"[SalaryStructure GetSalaryByEmployee] No salary found - EmployeeId: {employeeId}");
                    return NotFound(new ApiResponseModel<object>(false, "No salary found", null));
                }

                _logger.LogInformation($"[SalaryStructure GetSalaryByEmployee] Success - EmployeeId: {employeeId}");
                return Ok(new ApiResponseModel<SalaryResult>(
                    true,
                    "Success",
                    result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure GetSalaryByEmployee] Error - EmployeeId: {employeeId}, {ex.Message}\\nInner Exception: {ex?.InnerException?.Message}\\nStackTrace: {ex?.StackTrace}");
                return StatusCode(500, new ApiResponseModel<object>(
                    false,
                    $"Error retrieving employee salary: {ex.Message}",
                    null));
            }
        }


             [HttpGet("load-master")]

            public async Task<IActionResult> LoadMaster()
            {
                try
                {
                    _logger.LogInformation($"[SalaryStructure LoadMaster] Request received");
                    
                    var data = await _bal.LoadMaster();
                    
                    _logger.LogInformation($"[SalaryStructure LoadMaster] Success - Returned {data.Count} masters");
                    return Ok(new ApiResponseModel<List<SalaryComponentMaster>>(
                        true,
                        "Success",
                        data
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SalaryStructure LoadMaster] Error - {ex.Message}\\nInner Exception: {ex?.InnerException?.Message}\\nStackTrace: {ex?.StackTrace}");
                    return StatusCode(500, new ApiResponseModel<object>(
                        false,
                        $"Error loading master data: {ex.Message}",
                        null));
                }
            }



    }
}