namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IPermissionService
    {
        bool Has(string permissionKey);
        void Ensure(string permissionKey);

        int GetCurrentEmployeeId();
        bool IsSelf(int targetEmployeeId);

            // New: Contextual rules
        void EnsureNotSelf(int targetEmployeeId, string action);
        void EnsureCanActOn(string permissionKey, int targetEmployeeId);

        //void EnsureCanActOn(string permissionKey, object EmployeeId);
    }

}
