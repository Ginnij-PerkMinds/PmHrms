using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IDesignationBAL
    {
        Task<PagedResult<DesignationResponseModel>> GetAllDesignations(int page, int size, string? search, int orgId);
        Task<DesignationResponseModel?> GetDesignation(int id , int orgId);
        Task<DesignationResponseModel?> AddDesignation(DesignationModel request , int orgId,  int loggedInEmployeeId);
        Task<DesignationResponseModel?> UpdateDesignation(int id, DesignationModel request , int orgId);
        Task<bool> DeleteDesignation(int id);
        Task<List<DesignationResponseModel>> GetDesignationsByDepartment(int departmentId, int orgId);
       

    }
}