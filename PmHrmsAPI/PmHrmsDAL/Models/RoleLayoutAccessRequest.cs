namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class RoleLayoutAccessRequest
    {
        public int SystemRoleId { get; set; }
        public int LayoutId { get; set; }
        public bool IsAllowed { get; set; }

    }

}
