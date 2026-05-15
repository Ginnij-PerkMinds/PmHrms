using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsDAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeBankAccountController : ControllerBase
    {
        private readonly IEmployeeBankAccountBAL _bankAccountBAL;

        public EmployeeBankAccountController(IEmployeeBankAccountBAL bankAccountService)
        {
            _bankAccountBAL = bankAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> AddBankAccount([FromBody] CreateBankAccountModel createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<object>(false, "Validation Failed", ModelState));
            }
            try
            {
                var result = await _bankAccountBAL.AddBankAccountAsync(createDto);
                return Ok(new ApiResponseModel<BankAccountResponseModel>(true, "Bank account added successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBankAccount(int id, [FromBody] UpdateBankAccountModel updateDto)
        {
            if (id != updateDto.BankAccountId)
            {
                return BadRequest(new ApiResponseModel<string>(false, "BankAccountId mismatch", null));
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseModel<object>(false, "Validation Failed", ModelState));
            }
            try
            {
                var result = await _bankAccountBAL.UpdateBankAccountAsync(updateDto);
                if (result == null)
                {
                    return NotFound(new ApiResponseModel<string>(false, "Bank account not found", null));
                }
                return Ok(new ApiResponseModel<BankAccountResponseModel>(true, "Bank account updated successfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBankAccount(int id)
        {
            try
            {
                await _bankAccountBAL.DeleteBankAccountAsync(id);
                return Ok(new ApiResponseModel<string>(true, "Bank account deleted successfully (soft delete)", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error deleting bank account", ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBankAccountById(int id)
        {
            var result = await _bankAccountBAL.GetBankAccountByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponseModel<string>(false, "Bank account not found", null));
            }
            return Ok(new ApiResponseModel<BankAccountResponseModel>(true, "Success", result));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetBankAccountsByEmployeeId(int employeeId)
        {
            var result = await _bankAccountBAL.GetBankAccountsByEmployeeIdAsync(employeeId);
            return Ok(new ApiResponseModel<IEnumerable<BankAccountResponseModel>>(true, "Success", result));
        }
    }
}
