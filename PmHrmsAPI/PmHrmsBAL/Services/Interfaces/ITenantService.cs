namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface ITenantService
    {
        int GetOrgId();
        int GetCurrentUserID();
        int? GetDepartmentId();
        string GetUserRole();
    }

}
