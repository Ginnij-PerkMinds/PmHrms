using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    
   public interface IDepartmentBAL
    {
        Task<PagedResult<DepartmentResponseModel>> GetAllDepartments(int page, int size, string? search);
        Task<DepartmentResponseModel?> GetDepartment(int id);
        Task<DepartmentResponseModel?> AddDepartment(DepartmentModel request, int orgId, int loggedInEmployeeId);
        Task<DepartmentResponseModel?> UpdateDepartment(int id, DepartmentModel request);
        Task<bool> DeleteDepartment(int id);
    }
}