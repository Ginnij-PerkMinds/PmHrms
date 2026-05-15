using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsFAL.IRepositories;
using PmHrmsAPI.PmHrmsDAL.Utility;


namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class ExpenseBAL : IExpenseBAL
    {
        private readonly ExpenseDAL _expenseDAL;
        private readonly IDocumentFAL _documentFAL;

        public ExpenseBAL(ExpenseDAL expenseDAL, IDocumentFAL documentFAL)
        {
            _expenseDAL = expenseDAL;
            _documentFAL = documentFAL;
        }

        public async Task<ExpenseClaim> ApplyClaim(ExpenseClaimRequestModel model, int empId, int orgId)
        {
            var master = await _expenseDAL.GetMasterType(model.ExpenseTypeId);
            if (master == null || !master.IsActive)
                //throw new Exception("Invalid or inactive expense type.");
                throw new Exception(PmHrmsConstants.ExpenseMessages.InvalidExpenseType);

            var config = await _expenseDAL.GetOrgConfig(orgId, model.ExpenseTypeId);
            if (config != null && !config.IsEnabled) 
                //throw new Exception("This category is disabled by your organization.");
                throw new Exception(PmHrmsConstants.ExpenseMessages.CategoryDisabled);

            decimal effectiveLimit = config?.MaxLimit ?? master.DefaultMaxLimit;
            if (model.Amount > effectiveLimit) 
                //throw new Exception($"Claim amount {model.Amount} exceeds the allowed limit of {effectiveLimit}.");
                throw new Exception(PmHrmsConstants.ExpenseMessages.ClaimExceedsLimit);

            //string filePath = await _documentFAL.UploadDocumentAsync(model.Attachment, "ExpenseBillsPath");
            string filePath = await _documentFAL.UploadDocumentAsync(model.Attachment, PmHrmsConstants.FolderNames.Expenses);

            var claim = new ExpenseClaim
            {
                UserId = empId,
                OrganizationId = orgId,
                ExpenseTypeId = model.ExpenseTypeId,
                Amount = model.Amount,
                Description = model.Description,
                FilePath = filePath,
                //Status = "Pending",
                Status = PmHrmsConstants.ExpenseMessages.Pending,
                CreatedDate = DateTime.Now
            };

            await _expenseDAL.AddClaim(claim);
            return claim;
        }

        public async Task<List<ExpenseClaimResponseModel>> GetMyClaims(int empId)
        {
            var claims = await _expenseDAL.GetClaimsByEmp(empId);
            return claims.Select(x => new ExpenseClaimResponseModel
            {
                Id = x.Id,
                ExpenseTypeName = x.ExpenseType.TypeName,
                Amount = x.Amount,
                Status = x.Status,
                CreatedDate = x.CreatedDate,
                FilePath = x.FilePath,
                Description = x.Description,
                Remarks = x.Remarks
            }).OrderByDescending(x => x.CreatedDate).ToList();
        }

        public async Task<List<ExpenseClaimResponseModel>> GetAllClaims(int orgId, string? status)
        {
            var claims = await _expenseDAL.GetOrgClaims(orgId, status);
            return claims.Select(x => new ExpenseClaimResponseModel
            {
                Id = x.Id,
                EmployeeName = x.User.FirstName + " " + x.User.LastName,
                ExpenseTypeName = x.ExpenseType.TypeName,
                Amount = x.Amount,
                Status = x.Status,
                CreatedDate = x.CreatedDate,
                FilePath = x.FilePath,
                Description = x.Description
            }).ToList();
        }

        public async Task<bool> ApproveRejectClaim(int claimId, string status, string? remarks, int reviewedBy)
        {
            var claim = await _expenseDAL.GetClaimById(claimId);
            if (claim == null) return false;

            claim.Status = status;
            claim.Remarks = remarks;
            claim.ReviewedBy = reviewedBy;
            claim.ReviewedDate = DateTime.Now;

            await _expenseDAL.UpdateClaimStatus(claim);
            return true;
        }

        public async Task<List<object>> GetOrgExpenseConfig(int orgId)
        {
            return await _expenseDAL.GetFullConfigForOrg(orgId);
        }

        public async Task UpdateOrgConfig(ExpenseConfigUpdateModel model, int orgId)
        {
            var config = await _expenseDAL.GetOrgConfig(orgId, model.ExpenseTypeId);
            if (config == null)
            {
                config = new OrganizationExpenseConfig
                {
                    OrganizationId = orgId,
                    ExpenseTypeId = model.ExpenseTypeId,
                    CreatedDate = DateTime.Now
                };
                await _expenseDAL.AddOrgConfig(config);
            }
            config.MaxLimit = model.MaxLimit;
            config.IsEnabled = model.IsEnabled;
            await _expenseDAL.UpdateOrgConfig(config);
        }
    }
}