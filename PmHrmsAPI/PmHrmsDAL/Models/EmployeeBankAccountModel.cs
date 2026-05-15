namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class EmployeeBankAccountModel
    {
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public bool IsPrimary { get; set; }
    }
}
