using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;


namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IHolidayBAL
    {
        Task<object> GetMasterHolidays(int orgId, int year);
        Task<object> GetHolidayGroups(int orgId, int year);
        Task<object> SaveHolidayGroup(int orgId, SaveHolidayGroupRequest request);
        Task DeleteHolidayGroup(int orgId, int groupId);
        Task<object> AddCustomHoliday(int orgId, AddCustomHolidayRequest request);
        Task DeleteCustomHoliday(int orgId, int systemHolidayId);
        Task<object> GetEmployeeHolidays(int employeeId, int year);
    }
}
