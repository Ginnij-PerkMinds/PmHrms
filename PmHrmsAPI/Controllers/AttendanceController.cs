using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;

namespace PmHrmsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceBAL _attendanceBAL;
        private readonly AttendanceDAL _attendanceDAL;

        private readonly ILeaveBAL _leaveBAL;

        public AttendanceController(
            IAttendanceBAL attendanceBAL,
            AttendanceDAL attendanceDAL,
            ILeaveBAL leaveBAL)  
        {
            _attendanceBAL = attendanceBAL;
            _attendanceDAL = attendanceDAL;
            _leaveBAL = leaveBAL;
        }

      

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] LocationRequest request)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            var message = await _attendanceBAL
                .CheckInAsync(empId, request.Latitude, request.Longitude, ip!, device);

            return Ok(new { message });
        }

        //  CHECK OUT 

        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] LocationRequest request)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var message = await _attendanceBAL
                .CheckOutAsync(empId, request.Latitude, request.Longitude, ip!);

            return Ok(new { message });
        }

        [HttpPost("pause")]
        public async Task<IActionResult> Pause([FromBody] LocationRequest request)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            var message = await _attendanceBAL
                .PauseAsync(empId, request.Latitude, request.Longitude, ip!, device);

            return Ok(new { message });
        }

        [HttpPost("resume")]
        public async Task<IActionResult> Resume([FromBody] LocationRequest request)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            var message = await _attendanceBAL
                .ResumeAsync(empId, request.Latitude, request.Longitude, ip!, device);

            return Ok(new { message });
        }

        //  EMPLOYEE HISTORY 

        [HttpGet("history")]
        public async Task<IActionResult> GetAttendanceHistory(
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var result = await _attendanceBAL
                .GetAttendanceHistoryAsync(empId, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetAttendanceCalendar(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var result = await _attendanceBAL.GetAttendanceCalendarAsync(empId, year, month);

            return Ok(result);
        }

        //  TODAY STATUS 

        [HttpGet("today-status")]
        public async Task<IActionResult> GetTodayStatus()
        {
            var empIdClaim = User.FindFirst("EmployeeId")?.Value;


            if (string.IsNullOrEmpty(empIdClaim) || !int.TryParse(empIdClaim, out var empId))
            {
                return Unauthorized(new ApiResponseModel<string>(
                    false,
                    "Invalid or missing token claims",
                    null
                ));
            }

            var result = await _attendanceBAL
                .GetTodayAttendanceStateAsync(empId);

            return Ok(result);
        }

        [HttpGet("admin/today/summary")]
        public async Task<IActionResult> GetTodaySummary()
        {
            var orgId = GetOrganizationId();

            var result = await _attendanceBAL.GetTodaySummaryAsync(orgId);

            return Ok(result);
        }

        [HttpGet("admin/today")]
        public async Task<IActionResult> GetAdminTodayAttendance()
        {
            var orgId = GetOrganizationId();

            var employees = await _attendanceBAL
                .GetTodayAttendanceForAdminAsync(orgId);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var leaves = await _attendanceDAL.GetTodayApprovedLeaves(orgId); 

           
            var leaveMap = leaves
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var emp in employees)
            {
                if (leaveMap.TryGetValue(emp.EmployeeId, out var leave))
                {
                    emp.Status = "OnLeave";
                    emp.IsOnLeave = true;

                    emp.LeaveType = leave.LeaveType?.LeaveTypeName;
                    emp.Reason = leave.Reason;
                    emp.ApprovedBy = leave.ApprovedBy != null 
                        ? $"{leave.ApprovedBy.FirstName} {leave.ApprovedBy.LastName}".Trim()
                        : null;
                    emp.FromDate = leave.FromDate;
                    emp.ToDate = leave.ToDate;

                    emp.CheckInTime = null;
                    emp.CheckOutTime = null;
                }
                else
                {
                    emp.IsOnLeave = false;
                }
            }

            return Ok(employees);
        }

       

        [HttpGet("admin/{employeeId}/{date}/logs")]
        public async Task<IActionResult> GetEmployeeLogs(int employeeId, DateOnly date)
        {
            var logs = await _attendanceBAL
     .GetLogsWithISTAsync(employeeId, date);

            return Ok(logs);
        }

        [HttpGet("admin/calendar/{employeeId}")]
        public async Task<IActionResult> GetEmployeeCalendar(
    int employeeId,
    [FromQuery] int year,
    [FromQuery] int month)
        {
            var result = await _attendanceBAL.GetAttendanceCalendarAsync(employeeId, year, month);
            return Ok(result);
        }

        [HttpGet("admin/history/{employeeId}")]
        public async Task<IActionResult> GetEmployeeHistory(
       int employeeId,
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10)
        {
            var data = await _attendanceBAL
                .GetAttendanceHistoryAsync(employeeId, pageNumber, pageSize);

            return Ok(data);
        }
        // request for missed checkout
        [HttpPost("request")]
        public async Task<IActionResult> SubmitRequest([FromBody] SubmitAttendanceRequestModel model)
        {
            var empId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            await _attendanceBAL.SubmitMissedCheckoutRequest(
                empId,
                model.AttendanceId,
                model.Reason
            );

            return Ok(new
            {
                success = true,
                message = "Request submitted"
            });
        }


        // admin rq
        [HttpGet("admin/requests")]
        public async Task<IActionResult> GetRequests()
        {
            var data = await _attendanceBAL.GetAllAttendanceRequests();
            return Ok(data);
        }

        // approve
        [HttpPost("admin/approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveAttendanceRequestModel model)
        {
            await _attendanceBAL.ApproveRequest(
                model.RequestId,
                model.CheckoutTime
            );

            return Ok();
        }


        // reject
        [HttpPost("admin/reject")]
        public async Task<IActionResult> Reject([FromBody] RejectAttendanceRequestModel model)
        {
            await _attendanceBAL.RejectRequest(
                model.RequestId,
                model.Remarks
            );

            return Ok();
        }

        [HttpGet("admin/shifts")]
        public async Task<IActionResult> GetShifts(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 15)
        {
            var result = await _attendanceBAL.GetPagedShiftsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // HELPER 

        private int GetOrganizationId()
        {
            var claim = User.FindFirst("OrgId");

            if (claim == null)
                throw new UnauthorizedAccessException("OrgId claim not found.");

            return int.Parse(claim.Value);
        }
    }
}
