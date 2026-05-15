using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class EmployeeModel
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string OfficialEmail { get; set; } = string.Empty;

        [RegularExpression(@"^\+?[0-9]{10,15}$",
            ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        public string? AltPhoneNumber { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        public DateOnly DateOfJoining { get; set; }

        public string? EmploymentStatus { get; set; }

        public string? WorkMode { get; set; }

        public int? NoticePeriodDays { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int DesignationId { get; set; }

        public int? AssignedOfficeId { get; set; }

        public int? ShiftId { get; set; }

        public int? ReportingManagerId { get; set; }

        public int? SecondaryManagerId { get; set; }

        [Required]
        public int SystemRoleId { get; set; }

        [Required]
        public int OrgRoleId { get; set; }

        public DateOnly? ResignationDate { get; set; }

        public DateOnly? LastWorkingDay { get; set; }

        public string? ExitReason { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public int? PolicyId { get; set; }
    }
}
