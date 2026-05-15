namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class RoleBundleResponse
    {
        public List<RoleResponseModel> SystemRoles { get; set; } = new();
        public List<RoleResponseModel> OrgRoles { get; set; } = new();
    }

}
