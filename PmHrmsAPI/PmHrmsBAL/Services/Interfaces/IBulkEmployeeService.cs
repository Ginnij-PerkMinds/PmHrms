using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IBulkEmployeeService
    {
        Task BulkInsertAsync(
             int orgId,
             List<EmployeeModel> employees,
             int systemRoleId,
             int orgRoleId,
             string? sharedPassword = null);
    }

}
