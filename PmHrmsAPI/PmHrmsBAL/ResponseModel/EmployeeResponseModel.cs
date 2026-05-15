
using PmHrmsAPI.PmHrmsDAL.Models;
namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class EmployeeResponseModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public int OrganizationId { get; set; }

        public string OfficialEmail { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AltPhoneNumber { get; set; }

        public DateOnly DateOfJoining { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? WorkMode { get; set; }
        public int? NoticePeriodDays { get; set; }
        public bool? IsActive { get; set; }
        public string? OfficialImageUrl { get; set; }

   
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public int? AssignedOfficeId { get; set; }
        public int? ShiftId { get; set; }
        public int? ReportingManagerId { get; set; }
        public int? SecondaryManagerId { get; set; }

        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string? ReportingManagerName { get; set; }
        public string? SecondaryManagerName { get; set; }
        public int? SystemRoleId { get; set; }
        public string? SystemRoleName { get; set; }
        public int? OrgRoleId { get; set; }
        public string? OrgRoleName { get; set; }
        public int? PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string? ReportingManagerEmail { get; set; }
        public string? ReportingManagerPhone { get; set; }

        public string? ReportingManagerImageUrl { get; set; }
        public WorkPolicyInfo? WorkPolicy { get; set; }
        public OfficeLocationInfo? OfficeLocation {get ; set ;}

        public EmployeeDetailResponseModel? PersonalDetails { get; set; }
    }
}
