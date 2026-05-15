using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class AssignUserRoleRequest
    {
        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }

        [Range(1, int.MaxValue)]
        public int OrgRoleId { get; set; }
    }
}
