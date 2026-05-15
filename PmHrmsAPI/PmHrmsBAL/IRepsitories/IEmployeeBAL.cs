using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IEmployeeBAL
    {
        Task<PagedResult<EmployeeResponseModel>> GetAllEmployees(int page, int size, string? search);
        Task<EmployeeResponseModel?> GetEmployee(int id);
        Task<bool> PhoneExists(string phone);
        Task<List<EmployeeResponseModel>> GetTeamMembers(int employeeId);
        Task<OfficeLocationResult?> GetEmployeeOfficeLocation(int employeeId);
        Task<bool> EmailExists(string email);
        Task<bool> EmployeeCodeExists(string code, int orgId);
        Task<EmployeeResponseModel?> AddEmployee(EmployeeModel request);
        Task<EmployeeResponseModel?> UpdateEmployee(int id, EmployeeModel request);
        Task<EmployeeResponseModel?> UpdateAdminProfile(AdminProfileUpdateModel request);
        Task<RoleBundleResponse> GetRoles(int orgId);
        Task<string?> UpdateEmployeeProfileImage(int id, IFormFile file);
        Task<bool> DeleteEmployeeProfileImage(int id);

         Task<bool> DeleteEmployee(int id);
    }
}