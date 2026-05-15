using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDetailController : ControllerBase
    {
        private readonly IEmployeeDetailBAL _detailBAL;

        public EmployeeDetailController(IEmployeeDetailBAL detailBAL)
        {
            _detailBAL = detailBAL;
        }


        [HttpGet("ByEmployee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            var result = await _detailBAL.GetDetailByEmployeeId(employeeId);

          

            return Ok(new ApiResponseModel<object>(
                true,
                "Success",
                result 
            ));
        }


        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] EmployeeDetailModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseModel<object>(false, "Validation Failed", ModelState));

            try
            {
                var result = await _detailBAL.AddOrUpdateDetail(model);

                return Ok(new ApiResponseModel<EmployeeDetailResponseModel>(
                    true,
                    "Details saved successfully",
                    result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(
                    false,
                    ex.Message,
                    null));
            }
        }



        


    }
}