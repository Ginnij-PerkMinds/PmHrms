using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IEmployeeOnboardingService
    {
        Task ApplyDefaultsAsync(List<Employee> employees, int orgId);
        Task ApplyDefaultsAsync(Employee employee, int orgId);
    }
}