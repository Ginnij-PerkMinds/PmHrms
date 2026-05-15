using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<int> CreatePayrollRunAsync(CreatePayrollRequest model);
        Task<bool> StartPayrollAsync(int payrollRunId);
        Task<PayrollRunResponse> GetPayrollRunAsync(int payrollRunId);
        Task<List<PayrollRunListResponse>> GetPayrollRunsAsync(int orgId);
        Task<List<EmployeePayrollResponse>> GetEmployeePayrollsAsync(int payrollRunId);


        Task<EmployeePayrollResponse> GetEmployeePayrollAsync(int runId, int employeePayrollId);
        Task<IEnumerable<object>> GetRunDownloadRowsAsync(int runId);
        Task<PayrollConfigResponse> GetConfigurationAsync(int orgId);
        Task<PayrollConfigResponse> SaveConfigurationAsync(int orgId, UpdatePayrollConfigRequest model);
        Task<PayrollRunResponse> RecalculateRunAsync(int runId);
    }
}
