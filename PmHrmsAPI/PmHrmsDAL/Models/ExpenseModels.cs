using Microsoft.AspNetCore.Http;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class ExpenseClaimRequestModel
    {
        public int ExpenseTypeId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public IFormFile Attachment { get; set; } = null!;
    }

    public class ExpenseClaimResponseModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string ExpenseTypeName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class ExpenseConfigUpdateModel
    {
        public int ExpenseTypeId { get; set; }
        public decimal? MaxLimit { get; set; }
        public bool IsEnabled { get; set; }
    }
}