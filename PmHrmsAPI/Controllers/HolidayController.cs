using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using Hangfire;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayBAL _holidayBAL;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HolidayController(IHolidayBAL holidayBAL, IBackgroundJobClient backgroundJobClient)
        {
            _holidayBAL = holidayBAL;
            _backgroundJobClient = backgroundJobClient;
        }

        // ── 1. GET Master Catalog (Global + Org Custom) ──────────────────────
        [HttpGet("{orgId:int}/master-holidays")]
        public async Task<IActionResult> GetMasterHolidays(int orgId, [FromQuery] int year)
        {
            try
            {
                var data = await _holidayBAL.GetMasterHolidays(orgId, year > 0 ? year : DateTime.UtcNow.Year);
                return Ok(new ApiResponseModel<object>(true, "Success", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Failed to load master holidays", ex.Message));
            }
        }

        // ── 2. GET All Holiday Groups (Calendars) for an Org ─────────────────
        [HttpGet("{orgId:int}/groups")]
        public async Task<IActionResult> GetHolidayGroups(int orgId, [FromQuery] int year)
        {
            try
            {
                var data = await _holidayBAL.GetHolidayGroups(orgId, year > 0 ? year : DateTime.UtcNow.Year);
                return Ok(new ApiResponseModel<object>(true, "Success", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Failed to load groups", ex.Message));
            }
        }

        // ── 3. CREATE or UPDATE a Holiday Group ──────────────────────────────
        [HttpPost("{orgId:int}/groups")]
        public async Task<IActionResult> SaveHolidayGroup(int orgId, [FromBody] SaveHolidayGroupRequest request)
        {
            try
            {
                var data = await _holidayBAL.SaveHolidayGroup(orgId, request);
                return Ok(new ApiResponseModel<object>(true, "Group saved successfully", data));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Save failed", ex.Message));
            }
        }

        // ── 4. DELETE a Holiday Group ─────────────────────────────────────────
        [HttpDelete("{orgId:int}/groups/{groupId:int}")]
        public async Task<IActionResult> DeleteHolidayGroup(int orgId, int groupId)
        {
            try
            {
                await _holidayBAL.DeleteHolidayGroup(orgId, groupId);
                return Ok(new ApiResponseModel<string>(true, "Group deleted successfully", null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Delete failed", ex.Message));
            }
        }

        // ── 5. ADD a Custom Holiday to Master Catalog ─────────────────────────
        [HttpPost("{orgId:int}/custom")]
        public async Task<IActionResult> AddCustomHoliday(int orgId, [FromBody] AddCustomHolidayRequest request)
        {
            try
            {
                var data = await _holidayBAL.AddCustomHoliday(orgId, request);
                return Ok(new ApiResponseModel<object>(true, "Custom holiday added", data));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Add failed", ex.Message));
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeHolidays(int employeeId, [FromQuery] int year)
        {
            var data = await _holidayBAL.GetEmployeeHolidays(employeeId, year);
            return Ok(new ApiResponseModel<object>(true, "Success", data));
        }

        // ── 6. DELETE a Custom Holiday from Master Catalog ────────────────────
        [HttpDelete("{orgId:int}/custom/{systemHolidayId:int}")]
        public async Task<IActionResult> DeleteCustomHoliday(int orgId, int systemHolidayId)
        {
            try
            {
                await _holidayBAL.DeleteCustomHoliday(orgId, systemHolidayId);
                return Ok(new ApiResponseModel<string>(true, "Custom holiday deleted", null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Delete failed", ex.Message));
            }
        }

        // ── 7. TRIGGER Holiday Sync Job ───────────────────────────────────────
        [HttpPost("run-job")]
        public IActionResult RunHolidayJob([FromQuery] int year)
        {
            try
            {
                var targetYear = year > 0 ? year : DateTime.UtcNow.Year + 1;
                _backgroundJobClient.Enqueue<PmHrmsAPI.PmHrmsBAL.Jobs.HolidayAutomationJob>(
                    job => job.RunAsync(targetYear));

                return Ok(new ApiResponseModel<string>(true,
                    $"Holiday sync job queued for {targetYear}", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Failed to trigger job", ex.Message));
            }
        }
    }
}