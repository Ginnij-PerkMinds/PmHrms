using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;


namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface ILeaveBAL
    {
        Task<LeaveRequest> ApplyLeave(LeaveApplyModel model, int empId, int orgId);
        Task<bool> ApproveOrRejectLeave(LeaveApprovalModel model, int approvedById);
        Task<List<LeaveResponseModel>> GetMyLeaves(int empId);
        Task<List<LeaveResponseModel>> GetPendingRequests(int orgId);

        Task<List<LeaveTypeResponseModel>> GetLeaveTypes(int orgId);
        Task<LeaveType> CreateLeaveType(LeaveTypeModel model, int orgId);
        Task UpdateLeaveType(int id, LeaveTypeModel model, int orgId);
        Task DeleteLeaveType(int id);

        // Rules identified by rule_name (string), not int id
        Task<List<AllocationRuleResponseModel>> GetAllocationRules(int orgId);
        Task<AllocationRuleResponseModel> CreateAllocationRule(AllocationRuleModel model, int orgId);
        Task<AllocationRuleResponseModel> UpdateAllocationRule(string ruleName, AllocationRuleModel model, int orgId);
        Task DeleteAllocationRule(string ruleName, int orgId);
        Task AssignDesignations(string ruleName, List<int> designationIds, int orgId);

        Task<List<LeaveBalanceResponseModel>> GetLeaveBalance(int empId);
        Task<bool> CancelLeave(int leaveId, int empId);
        Task<List<LeaveMasterResponseModel>> GetLeaveMasterList();
    }
}