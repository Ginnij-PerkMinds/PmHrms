using System;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class BankAccountResponseModel
    {
        public int BankAccountId { get; set; }
        public int EmployeeId { get; set; }
        public int OrganizationId { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankName { get; set; }
        public string? BranchName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
