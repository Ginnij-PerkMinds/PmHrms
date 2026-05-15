using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IExpenseBAL
    {
        Task<ExpenseClaim> ApplyClaim(ExpenseClaimRequestModel model, int empId, int orgId);
        Task<List<ExpenseClaimResponseModel>> GetMyClaims(int empId);
        Task<List<ExpenseClaimResponseModel>> GetAllClaims(int orgId, string? status);
        Task<bool> ApproveRejectClaim(int claimId, string status, string? remarks, int reviewedBy);
        Task<List<object>> GetOrgExpenseConfig(int orgId);
        Task UpdateOrgConfig(ExpenseConfigUpdateModel model, int orgId);
    }
}