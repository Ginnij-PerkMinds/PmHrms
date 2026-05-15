using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveBAL _leaveBAL;
        public LeaveController(ILeaveBAL leaveBAL) => _leaveBAL = leaveBAL;

        private int EmpId => int.Parse(User.FindFirst("EmployeeId")!.Value);
        private int OrgId => int.Parse(User.FindFirst("OrgId")!.Value);

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] LeaveApplyModel model)
        {
            var leave = await _leaveBAL.ApplyLeave(model, EmpId, OrgId);

            var response = new LeaveApplyResponseModel
            {
                LeaveId = leave.LeaveId,
                LeaveTypeId = leave.LeaveTypeId,
                Reason = leave.Reason,
                TotalDays = leave.TotalDays,
                AppliedAt = leave.AppliedAt
            };

            return Ok(new ApiResponseModel<LeaveApplyResponseModel>(true, "Submitted", response));
        }

        [HttpPut("approve-reject")]
        public async Task<IActionResult> ApproveReject([FromBody] LeaveApprovalModel model)
        {
            if (!await _leaveBAL.ApproveOrRejectLeave(model, EmpId)) return NotFound();
            return Ok(new { message = "Status updated" });
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory() =>
            Ok(await _leaveBAL.GetMyLeaves(EmpId));

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPending() =>
            Ok(await _leaveBAL.GetPendingRequests(OrgId));

        [HttpGet("types")]
        public async Task<IActionResult> GetLeaveTypes() =>
            Ok(new ApiResponseModel<List<LeaveTypeResponseModel>>(true, "Success",
                await _leaveBAL.GetLeaveTypes(OrgId)));

        [HttpPost("types")]
        public async Task<IActionResult> CreateType([FromBody] LeaveTypeModel model) =>
            Ok(new ApiResponseModel<LeaveType>(true, "Created",
                await _leaveBAL.CreateLeaveType(model, OrgId)));

        [HttpPut("types/{id:int}")]
        public async Task<IActionResult> UpdateType(int id, [FromBody] LeaveTypeModel model)
        {
            await _leaveBAL.UpdateLeaveType(id, model, OrgId);
            return Ok(new { message = "Updated" });
        }

        [HttpDelete("types/{id:int}")]
        public async Task<IActionResult> DeleteType(int id)
        {
            await _leaveBAL.DeleteLeaveType(id);
            return Ok(new { message = "Deleted" });
        }

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules() =>
            Ok(new ApiResponseModel<List<AllocationRuleResponseModel>>(true, "Success",
                await _leaveBAL.GetAllocationRules(OrgId)));

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] AllocationRuleModel model) =>
            Ok(new ApiResponseModel<AllocationRuleResponseModel>(true, "Created",
                await _leaveBAL.CreateAllocationRule(model, OrgId)));

        [HttpPut("rules/{ruleName}")]
        public async Task<IActionResult> UpdateRule(string ruleName, [FromBody] AllocationRuleModel model) =>
            Ok(new ApiResponseModel<AllocationRuleResponseModel>(true, "Updated",
                await _leaveBAL.UpdateAllocationRule(ruleName, model, OrgId)));

        [HttpDelete("rules/{ruleName}")]
        public async Task<IActionResult> DeleteRule(string ruleName)
        {
            await _leaveBAL.DeleteAllocationRule(ruleName, OrgId);
            return Ok(new { message = "Deleted" });
        }

        [HttpPost("rules/{ruleName}/designations")]
        public async Task<IActionResult> AssignDesignations(
            string ruleName, [FromBody] AssignDesignationsModel model)
        {
            await _leaveBAL.AssignDesignations(ruleName, model.DesignationIds, OrgId);
            return Ok(new { message = "Assigned" });
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetMyBalance() =>
            Ok(new ApiResponseModel<List<LeaveBalanceResponseModel>>(true, "Success",
                await _leaveBAL.GetLeaveBalance(EmpId)));

        [HttpGet("master-list")]
        public async Task<IActionResult> GetMasterList()
        {
            var result = await _leaveBAL.GetLeaveMasterList();
            return Ok(new ApiResponseModel<List<LeaveMasterResponseModel>>(true, "Success", result));
        }

        [HttpPut("cancel/{leaveId:int}")]
        public async Task<IActionResult> Cancel(int leaveId)
        {
            if (!await _leaveBAL.CancelLeave(leaveId, EmpId))
                return BadRequest(new { message = "Cannot cancel — not found, not yours, or not pending." });

            return Ok(new { message = "Leave cancelled" });
        }
}
}