using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class CreateBankAccountModel
    {
        [Required(ErrorMessage = "EmployeeId is required.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "OrganizationId is required.")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "AccountHolderName is required.")]
        [StringLength(255, ErrorMessage = "AccountHolderName cannot exceed 255 characters.")]
        public string AccountHolderName { get; set; }

        [Required(ErrorMessage = "AccountNumber is required.")]
        [StringLength(50, ErrorMessage = "AccountNumber cannot exceed 50 characters.")]
        // Basic validation for account number format, can be enhanced
        [RegularExpression(@"^[0-9]{9,18}$", ErrorMessage = "Invalid Account Number format.")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "IFSCCode is required.")]
        [StringLength(20, ErrorMessage = "IFSCCode cannot exceed 20 characters.")]
        // Basic validation for IFSC code format, can be enhanced
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC Code format.")]
        public string IFSCCode { get; set; }

        [Required(ErrorMessage = "BankName is required.")]
        [StringLength(255, ErrorMessage = "BankName cannot exceed 255 characters.")]
        public string BankName { get; set; }

        [StringLength(255, ErrorMessage = "BranchName cannot exceed 255 characters.")]
        public string? BranchName { get; set; }

        public bool IsPrimary { get; set; }
    }
}
