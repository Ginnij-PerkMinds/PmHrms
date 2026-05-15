using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;

namespace PmHrmsAPI.Controllers
{
    [ApiController]
    [Route("api/payroll")]
    public class PayrollController : ControllerBase
    {         private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpPost("run")]
        public async Task<IActionResult> CreateRun([FromBody] CreatePayrollRequest model)
        {
            var id = await _payrollService.CreatePayrollRunAsync(model);
            return Ok(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<int>(true, "Payroll run created", id));
        }

        [HttpPost("run/{id}/start")]
        public async Task<IActionResult> StartRun([FromRoute] int id)
        {
            var ok = await _payrollService.StartPayrollAsync(id);
            if (!ok) return BadRequest(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<string>(false, "Unable to start payroll run", string.Empty));
            return Ok(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<string>(true, "Payroll started", string.Empty));
        }

        [HttpGet("run/{id}")]
        public async Task<IActionResult> GetRun([FromRoute] int id)
        {
            var run = await _payrollService.GetPayrollRunAsync(id);
            if (run == null) return NotFound(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<string>(false, "Not found", string.Empty));
            return Ok(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<PayrollRunResponse>(true, "", run));
        }

        [HttpGet("runs")]
        public async Task<IActionResult> GetRuns([FromQuery] int orgId)
        {
            var runs = await _payrollService.GetPayrollRunsAsync(orgId);
            return Ok(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<List<PayrollRunListResponse>>(true, "", runs));
        }

        [HttpGet("run/{id}/employees")]
        public async Task<IActionResult> GetEmployees([FromRoute] int id)
        {
            var emps = await _payrollService.GetEmployeePayrollsAsync(id);
            return Ok(new PmHrmsAPI.PmHrmsBAL.ResponseModel.ApiResponseModel<List<EmployeePayrollResponse>>(true, "", emps));
        }


       

        [HttpGet("run/{runId}/employee/{employeePayrollId}")]
        public async Task<IActionResult> GetEmployeePayroll([FromRoute] int runId, [FromRoute] int employeePayrollId)
        {
            
            var emp = await _payrollService.GetEmployeePayrollAsync(runId, employeePayrollId);
            if (emp == null) return NotFound(new ApiResponseModel<string>(false, "Not found", string.Empty));
            return Ok(new ApiResponseModel<EmployeePayrollResponse>(true, "", emp));
        }

        [HttpGet("run/{id}/download")]
        public async Task<IActionResult> DownloadRun([FromRoute] int id)
        {
            
            var rows = await _payrollService.GetRunDownloadRowsAsync(id);
            return Ok(new ApiResponseModel<IEnumerable<object>>(true, "", rows));
        }

        [HttpGet("configuration")]
        public async Task<IActionResult> GetConfiguration([FromQuery] int orgId)
        {
           
            var config = await _payrollService.GetConfigurationAsync(orgId);
            return Ok(new ApiResponseModel<PayrollConfigResponse>(true, "", config));
        }

        [HttpPost("configuration/{orgId}")]
        public async Task<IActionResult> SaveConfiguration([FromRoute] int orgId, [FromBody] UpdatePayrollConfigRequest model)
        {
            
            var config = await _payrollService.SaveConfigurationAsync(orgId, model);
            return Ok(new ApiResponseModel<PayrollConfigResponse>(true, "Configuration saved", config));
        }

        [HttpPost("run/{id}/recalculate")]
        public async Task<IActionResult> RecalculateRun([FromRoute] int id)
        {
           
            var run = await _payrollService.RecalculateRunAsync(id);
            return Ok(new ApiResponseModel<PayrollRunResponse>(true, "Payroll recalculated", run));
        }
    }
}
