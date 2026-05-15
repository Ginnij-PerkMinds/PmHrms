using System.ComponentModel.DataAnnotations;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class LeaveApplyModel
    {
        [Required]
        public int LeaveTypeId { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class LeaveApprovalModel
    {
        [Required]
        public int LeaveId { get; set; }
        [Required]
        public LeaveStatus Status { get; set; }
    }

    public class LeaveResponseModel
    {
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }

        public string? ApprovedBy { get; set; }

      
    }

    
}