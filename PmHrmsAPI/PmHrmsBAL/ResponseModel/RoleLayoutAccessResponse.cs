namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class RoleLayoutAccessResponse
    {
        public int? SystemRoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public int LayoutId { get; set; }
        public string LayoutKey { get; set; } = string.Empty;
        public string LayoutName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }
    }

}
