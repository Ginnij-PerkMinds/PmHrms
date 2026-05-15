namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class UserRoleAssignmentModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string OfficialEmail { get; set; } = string.Empty;
        public int? OrgRoleId { get; set; }
        public string? OrgRoleName { get; set; }
    }
}
