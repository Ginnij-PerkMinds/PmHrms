using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IEmployeeDetailBAL
    {
        Task<EmployeeDetailResponseModel?> GetDetailByEmployeeId(int employeeId);
        Task<EmployeeDetailResponseModel?> AddOrUpdateDetail(EmployeeDetailModel request);
    }
}