namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class RolePermissionRequest
    {
        public int OrgRoleId { get; set; }

        
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}