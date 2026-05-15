using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IAttendanceBAL
    {
        Task<AdminAttendanceSummaryResponse> GetTodaySummaryAsync(int organizationId);

        Task<string> CheckInAsync(int employeeId, double latitude, double longitude, string ip, string device);
        Task<string> PauseAsync(int employeeId, double latitude, double longitude, string ip, string device);
        Task<string> ResumeAsync(int employeeId, double latitude, double longitude, string ip, string device);
        Task<string> CheckOutAsync(int employeeId, double latitude, double longitude, string ip);

        Task<AttendanceStateResponse> GetTodayAttendanceStateAsync(int employeeId);

        Task<List<AttendanceLog>> GetLogsWithISTAsync(int employeeId, DateOnly date);

        // ✅ UPDATED (pagination)
        Task<PagedResult<AttendanceResponseModel>>
            GetAttendanceHistoryAsync(int employeeId, int page, int size);

        Task<List<AttendanceCalendarDayResponse>> GetAttendanceCalendarAsync(int employeeId, int year, int month);

        Task<List<AdminAttendanceResponse>> GetTodayAttendanceForAdminAsync(int organizationId);

        Task SubmitMissedCheckoutRequest(int employeeId, int attendanceId, string reason);
        Task<List<AttendanceRequestResponse>> GetAllAttendanceRequests();
        Task ApproveRequest(Guid requestId, DateTime checkoutTime);
        Task RejectRequest(Guid requestId, string remarks);
        Task<PagedResult<ShiftMaster>> GetPagedShiftsAsync(int pageNumber, int pageSize);
    }
}
