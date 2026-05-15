using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseBAL _expenseBAL;
        private readonly IPermissionService _permissionService;
        private readonly ITenantService _tenantService;

        public ExpenseController(
            IExpenseBAL expenseBAL,
            IPermissionService permissionService,
            ITenantService tenantService)
        {
            _expenseBAL = expenseBAL;
            _permissionService = permissionService;
            _tenantService = tenantService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromForm] ExpenseClaimRequestModel model)
        {
            //_permissionService.Ensure(PermissionKeys.EXP_APPLY);

            int empId = _permissionService.GetCurrentEmployeeId();
            int orgId = _tenantService.GetOrgId();

            var result = await _expenseBAL.ApplyClaim(model, empId, orgId);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            //_permissionService.Ensure(PermissionKeys.EXP_APPLY);

            int empId = _permissionService.GetCurrentEmployeeId();
            var claims = await _expenseBAL.GetMyClaims(empId);
            return Ok(claims);
        }

        [HttpGet("admin/all-claims")]
        public async Task<IActionResult> GetAllClaims(string? status)
        {
            _permissionService.Ensure(PermissionKeys.EXP_APPROVE_DENY);

            int orgId = _tenantService.GetOrgId();
            var claims = await _expenseBAL.GetAllClaims(orgId, status);
            return Ok(claims);
        }

        [HttpPut("approve-reject")]
        public async Task<IActionResult> ApproveReject(int id, string status, string? remarks)
        {
            _permissionService.Ensure(PermissionKeys.EXP_APPROVE_DENY);

            int reviewerId = _permissionService.GetCurrentEmployeeId();
            var res = await _expenseBAL.ApproveRejectClaim(id, status, remarks, reviewerId);
            return Ok(new { success = res });
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            //_permissionService.Ensure(PermissionKeys.EXP_CONFIG_MANAGE);

            int orgId = _tenantService.GetOrgId();
            var config = await _expenseBAL.GetOrgExpenseConfig(orgId);
            return Ok(config);
        }

        [HttpPost("config/update")]
        public async Task<IActionResult> UpdateConfig(ExpenseConfigUpdateModel model)
        {
            _permissionService.Ensure(PermissionKeys.EXP_CONFIG_MANAGE);

            int orgId = _tenantService.GetOrgId();
            await _expenseBAL.UpdateOrgConfig(model, orgId);
            return Ok(new { success = true });
        }
    }
}