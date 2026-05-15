using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using System.Linq;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Helpers;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class AttendanceBAL : IAttendanceBAL
    {
        private const int AlmostCompleteThresholdMinutes = 30;
        private const string WorkingStatusLabel = "Working";
        private const string OnLeaveStatusLabel = "OnLeave";
        private const string HolidayStatusLabel = "Holiday";
        private const string WeekOffStatusLabel = "WeekOff";
        private static readonly IReadOnlyDictionary<DateOnly, string> EmptyHolidayLookup
            = new Dictionary<DateOnly, string>();

        private readonly AttendanceDAL _attendanceDAL;
        //private readonly PmHrmsContext _context;                  
        private readonly IGeoService _geoService;
        private readonly RemoteLocationDAL _remoteLocationDAL;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AttendanceBAL> _log;
        private readonly HolidayDAL _holidayDAL;

        public AttendanceBAL(
            AttendanceDAL attendanceDAL,
            //PmHrmsContext context,
            IGeoService geoService,
            RemoteLocationDAL remoteLocationDAL,
            IPermissionService permissionService,
            HolidayDAL holidayDAL,
            ILogger<AttendanceBAL> log)
        {
            _attendanceDAL = attendanceDAL;
            //_context = context;
            _geoService = geoService;
            _remoteLocationDAL = remoteLocationDAL;
            _permissionService = permissionService;
            _holidayDAL = holidayDAL;
            _log = log;
        }

        #region Core Attendance Actions

        public async Task<string> CheckInAsync(int employeeId, double lat, double lng, string ip, string device)
        {
            var now = DateTime.UtcNow;
            var today = GetCurrentAttendanceDate();

            var isOnLeave = await _attendanceDAL.IsEmployeeOnLeave(employeeId, today);

            if (isOnLeave)
            {
                //throw new Exception("You are on leave today");
                throw new Exception(AttendanceMessages.OnLeaveToday);
            }

            _log.LogInformation("[Attendance] Starting Check-in for EmpID: {EmployeeId}. Device: {Device}, IP: {Ip}", employeeId, device, ip);

            var existing = await _attendanceDAL.GetTodayAttendanceAsync(employeeId, today);
            if (existing != null)
            {
                _log.LogWarning("[Attendance] Duplicate check-in attempt for EmpID: {EmployeeId} on {Today}", employeeId, today);
                //throw new Exception("Already checked in today.");
                throw new Exception(AttendanceMessages.AlreadyCheckedInToday);
            }

            var employee = await _attendanceDAL.GetEmployeeWithPolicyAsync(employeeId);
            if (employee == null)
            {
                //throw new Exception("Employee not found.");
                throw new Exception(AttendanceMessages.EmployeeNotFound);
            }

            var policy = await ResolveEffectivePolicyAsync(employee) ?? 
                //throw new Exception("Employee or Work Policy not found.");
                throw new Exception(AttendanceMessages.PolicyNotFound);

            await EnsureLocationAllowedAsync(employee, policy, employeeId, lat, lng);

            var status = ResolveCheckInStatus(policy, now);
            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                OrganizationId = employee.OrganizationId,
                AttendanceDate = today,
                CheckInTime = now,
                CheckInLatitude = (decimal)lat,
                CheckInLongitude = (decimal)lng,
                CheckInIp = ip,
                Status = status,
                CreatedAt = now
            };

            await _attendanceDAL.AddAsync(attendance);
            await _attendanceDAL.AddLogAsync(CreateAttendanceLog(employeeId, employee.OrganizationId, AttendanceLogType.CheckIn, now, lat, lng, ip, device));
            await _attendanceDAL.SaveChangesAsync();

            _log.LogInformation("[Attendance] Check-in SUCCESS for EmpID: {EmployeeId}. Status: {Status}", employeeId, status);

            //return status == AttendanceStatus.Late ? "Check-in successful (Marked Late)." : "Check-in successful.";
            return status == AttendanceStatus.Late ? AttendanceMessages.CheckInLateSuccess : AttendanceMessages.CheckInSuccess;
        }

        public async Task<string> PauseAsync(int employeeId, double lat, double lng, string ip, string device)
        {
            var now = DateTime.UtcNow;

            var (attendance, _, policy) = await GetOpenAttendanceContextAsync(employeeId, GetCurrentAttendanceDate());
            var logs = await _attendanceDAL.GetAttendanceLogsAsync(employeeId, attendance.AttendanceDate, attendance.AttendanceDate.AddDays(1));
            var breakSummary = BuildBreakSummary(logs, now);
            var requiredWorkingMinutes = GetRequiredWorkingMinutes(policy);
            var workedMinutes = CalculateWorkedMinutes(attendance.CheckInTime, now, policy, breakSummary.TotalBreakMinutes);
            var remainingWorkingMinutes = Math.Max(requiredWorkingMinutes - workedMinutes, 0);

            EnsurePauseAllowed(policy, breakSummary, remainingWorkingMinutes);

            await _attendanceDAL.AddLogAsync(CreateAttendanceLog(employeeId, attendance.OrganizationId, AttendanceLogType.Pause, now, lat, lng, ip, device));
            await _attendanceDAL.SaveChangesAsync();

            var remainingBreakMinutes = GetRemainingBreakMinutes(policy, breakSummary.TotalBreakMinutes);
            //return remainingBreakMinutes > 0 ? $"Pause started. {remainingBreakMinutes} break minutes remaining." : "Pause started.";
            return remainingBreakMinutes > 0 ? $"{AttendanceMessages.PauseStarted} {remainingBreakMinutes} break minutes remaining." : AttendanceMessages.PauseStarted;
        }

        public async Task<string> ResumeAsync(int employeeId, double lat, double lng, string ip, string device)
        {
            var now = DateTime.UtcNow;

            var (attendance, _, policy) = await GetOpenAttendanceContextAsync(employeeId, GetCurrentAttendanceDate());
            var logs = await _attendanceDAL.GetAttendanceLogsAsync(employeeId, attendance.AttendanceDate, attendance.AttendanceDate.AddDays(1));
            var breakSummary = BuildBreakSummary(logs, now);

            if (!breakSummary.IsPaused)
                //throw new Exception("Attendance is not paused.");
                throw new Exception(AttendanceMessages.AttendanceNotPaused);

            await _attendanceDAL.AddLogAsync(CreateAttendanceLog(employeeId, attendance.OrganizationId, AttendanceLogType.Resume, now, lat, lng, ip, device));
            await _attendanceDAL.SaveChangesAsync();

            var allowedBreakMinutes = GetAllowedBreakMinutes(policy);
            var overLimitMinutes = Math.Max(breakSummary.TotalBreakMinutes - allowedBreakMinutes, 0);
            if (overLimitMinutes > 0)
                //return $"Work resumed. Break limit exceeded by {overLimitMinutes} minutes.";
                return $"{AttendanceMessages.ResumeWork} Break limit exceeded by {overLimitMinutes} minutes.";

            var remainingBreakMinutes = GetRemainingBreakMinutes(policy, breakSummary.TotalBreakMinutes);
            //return remainingBreakMinutes > 0 ? $"Work resumed. {remainingBreakMinutes} break minutes remaining." : "Work resumed.";
            return remainingBreakMinutes > 0 ? $"{AttendanceMessages.ResumeWork} {remainingBreakMinutes} break minutes remaining." : AttendanceMessages.ResumeWork;
        }

        public async Task<string> CheckOutAsync(int employeeId, double lat, double lng, string ip)
        {
            var now = DateTime.UtcNow;

            var (attendance, _, policy) = await GetOpenAttendanceContextAsync(employeeId, GetCurrentAttendanceDate());

            attendance.CheckOutTime = now;
            attendance.CheckOutLatitude = (decimal)lat;
            attendance.CheckOutLongitude = (decimal)lng;
            attendance.CheckOutIp = ip;

            var logs = await _attendanceDAL.GetAttendanceLogsAsync(employeeId, attendance.AttendanceDate, attendance.AttendanceDate.AddDays(1));
            var breakSummary = BuildBreakSummary(logs, now);
            attendance.TotalWorkingMinutes = CalculateWorkedMinutes(attendance.CheckInTime, now, policy, breakSummary.TotalBreakMinutes);
            attendance.Status = ResolveCheckoutStatus(attendance.Status, policy, attendance.TotalWorkingMinutes);

            await _attendanceDAL.UpdateAsync(attendance);
            await _attendanceDAL.AddLogAsync(CreateAttendanceLog(employeeId, attendance.OrganizationId, AttendanceLogType.CheckOut, now, lat, lng, ip, null));
            await _attendanceDAL.SaveChangesAsync();

            var remainingWorkingMinutes = Math.Max(GetRequiredWorkingMinutes(policy) - attendance.TotalWorkingMinutes, 0);
            //return remainingWorkingMinutes > 0 ? $"Check-out successful. {remainingWorkingMinutes} minutes short of the policy requirement." : "Check-out successful.";
            return remainingWorkingMinutes > 0 ? $"{AttendanceMessages.CheckOutSuccess} {remainingWorkingMinutes} minutes short of the policy requirement." : AttendanceMessages.CheckOutSuccess;
        }

        #endregion

        #region Queries & State

        public async Task<PagedResult<AttendanceResponseModel>>
  GetAttendanceHistoryAsync(int employeeId, int page, int size)
        {
            var istZone = GetAttendanceTimeZone();
            var today = GetCurrentAttendanceDate();
            var employee = await _attendanceDAL.GetEmployeeWithPolicyAsync(employeeId);
            var policy = employee != null ? await ResolveEffectivePolicyAsync(employee) : null;
            var doj = employee.DateOfJoining;

            //var query = _context.Attendances
            //    .Where(a => a.EmployeeId == employeeId &&
            //                a.AttendanceDate >= doj)
            //    .OrderByDescending(a => a.AttendanceDate);

            //var totalCount = await query.CountAsync();
            var totalCount = await _attendanceDAL.GetAttendanceHistoryCountAsync(employeeId, doj);

            //var records = await query
            //    .Skip((page - 1) * size)
            //    .Take(size)
            //    .ToListAsync();
            var records = await _attendanceDAL.GetAttendanceHistoryAsync(employeeId, doj, page, size);

            var uniqueYears = records.Select(r => r.AttendanceDate.Year).Distinct().ToList();

            var groupsByYear = await Task.WhenAll(
            uniqueYears.Select(async y => new
            {
                Year = y,
                Groups = await _holidayDAL.GetActiveGroupsWithDetailsAsync(employee.OrganizationId, y)
            }));

            var holidayLookupsByYear = groupsByYear.ToDictionary(
                x => x.Year,
                x => BuildHolidayLookupForEmployee(employee, x.Year, x.Groups));


            //var requests = await _context.AttendanceRequests
            //    .Where(r => r.EmployeeId == employeeId &&
            //r.Status == "PENDING" || r.Status == "APPROVED" || r.Status == "REJECTED")
            //    .ToListAsync();
            var requests = await _attendanceDAL.GetAttendanceRequestsByEmployeeAsync(employeeId);

            //var leaves = await _context.LeaveRequests
            //    .Where(l => l.EmployeeId == employeeId && l.Status == (int)LeaveStatus.Approved)
            //    .Include(l => l.LeaveType)
            //    .Include(l => l.ApprovedBy)
            //    .ToListAsync();
            var leaves = await _attendanceDAL.GetApprovedLeavesByEmployeeAsync(employeeId, doj, doj.AddYears(1));

            var items = records.Select(a =>
            {
                var date = a.AttendanceDate;
                var request = requests
                    .FirstOrDefault(r => r.AttendanceId == a.AttendanceId);
                var leaveRequest = leaves
                    .FirstOrDefault(l => l.FromDate <= date && l.ToDate >= date);
                var leave = leaveRequest != null
                    ? new ApprovedLeaveDayInfo(
                        leaveRequest.LeaveType?.LeaveTypeName,
                        BuildApproverName(leaveRequest.ApprovedBy))
                    : null;
                var holidayLookup = holidayLookupsByYear.GetValueOrDefault(date.Year, EmptyHolidayLookup);
                var nonWorkingDay = ResolveNonWorkingDayInfo(date, policy, holidayLookup);
                var status = ResolveDayStatus(date, today, a, leave, nonWorkingDay);

                bool canRequest =
                    a.AttendanceDate != today &&
                    a.CheckInTime != null &&
                    a.CheckOutTime == null;

                return new AttendanceResponseModel
                {
                    AttendanceId = a.AttendanceId,
                    AttendanceDate = a.AttendanceDate,
                    CheckInTime = a.CheckInTime.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(a.CheckInTime.Value, istZone)
                        : null,
                    CheckOutTime = a.CheckOutTime.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(a.CheckOutTime.Value, istZone)
                        : null,
                    Status = status,
                    IsOnLeave = leaveRequest != null,
                    LeaveType = leave?.LeaveTypeName,
                    ApprovedBy = leave?.ApprovedBy,
                    IsHoliday = nonWorkingDay.IsHoliday,
                    HolidayName = nonWorkingDay.HolidayName,
                    IsWeekOff = nonWorkingDay.IsWeekOff,
                    IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                    WeekOffLabel = nonWorkingDay.WeekOffLabel,
                    TotalWorkingMinutes = a.TotalWorkingMinutes,
                    CanRequestCorrection = canRequest,
                    RequestStatus = request?.Status,
                    IsWorking = status == WorkingStatusLabel
                };
            }).ToList();

            return new PagedResult<AttendanceResponseModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        public async Task<List<AttendanceCalendarDayResponse>> GetAttendanceCalendarAsync(int employeeId, int year, int month)
        {
            if (month is < 1 or > 12)
            {
//validation messages are fine
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
            }

            var employee = await _attendanceDAL.GetEmployeeWithPolicyAsync(employeeId)
                ?? throw new Exception("Employee not found.");

            var policy = await ResolveEffectivePolicyAsync(employee);

            var holidayLookup = await BuildHolidayLookupAsync(employee, year);

            var istZone = GetAttendanceTimeZone();
            var today = GetCurrentAttendanceDate();


            var doj = employee.DateOfJoining;

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var lastDate = endDate.AddDays(-1);

            //var requests = await _context.AttendanceRequests
            //    .Where(r => r.EmployeeId == employeeId)
            //    .ToListAsync();
            var requests = await _attendanceDAL.GetAttendanceRequestsByEmployeeAsync(employeeId);

            //var attendances = await _context.Attendances
            //    .Where(a => a.EmployeeId == employeeId &&
            //                a.AttendanceDate >= startDate &&
            //                a.AttendanceDate <= lastDate)
            //    .ToListAsync();
            var attendances = await _attendanceDAL.GetAttendanceByDateRangeAsync(employeeId, startDate, lastDate);

            //var leaves = await _context.LeaveRequests
            //    .Where(l => l.EmployeeId == employeeId &&
            //                l.Status == (int)LeaveStatus.Approved &&
            //                l.FromDate <= lastDate &&
            //                l.ToDate >= startDate)
            //    .Include(l => l.LeaveType)
            //    .Include(l => l.ApprovedBy)
            //    .ToListAsync();
            var leaves = await _attendanceDAL.GetApprovedLeavesByEmployeeAsync(employeeId, startDate, lastDate);

            var attendanceByDate = attendances.ToDictionary(a => a.AttendanceDate);
            var leaveByDate = BuildLeaveLookup(leaves, startDate, lastDate);

            var items = new List<AttendanceCalendarDayResponse>();

            for (var date = startDate; date <= lastDate; date = date.AddDays(1))
            {

                if (date < employee.DateOfJoining)
                    continue;

                attendanceByDate.TryGetValue(date, out var attendance);
                var request = requests
               .FirstOrDefault(r => r.AttendanceId == attendance?.AttendanceId);
                leaveByDate.TryGetValue(date, out var leave);

                var nonWorkingDay = ResolveNonWorkingDayInfo(date, policy, holidayLookup);
                var status = ResolveDayStatus(date, today, attendance, leave, nonWorkingDay);

                items.Add(new AttendanceCalendarDayResponse
                {

                    AttendanceId = attendance?.AttendanceId,
                    AttendanceDate = date,
                    Status = status,
                    CheckInTime = attendance?.CheckInTime.HasValue == true
                        ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckInTime.Value, istZone)
                        : null,
                    CheckOutTime = attendance?.CheckOutTime.HasValue == true
                        ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckOutTime.Value, istZone)
                        : null,
                    TotalWorkingMinutes = attendance?.TotalWorkingMinutes ?? 0,
                    IsOnLeave = leave != null,
                    LeaveType = leave?.LeaveTypeName,
                    ApprovedBy = leave?.ApprovedBy,
                    IsHoliday = nonWorkingDay.IsHoliday,
                    HolidayName = nonWorkingDay.HolidayName,
                    IsWeekOff = nonWorkingDay.IsWeekOff,
                    IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                    WeekOffLabel = nonWorkingDay.WeekOffLabel,
                    RequestStatus = request?.Status
                });
            }

            return items;
        }

        public async Task<AttendanceStateResponse> GetTodayAttendanceStateAsync(int employeeId)
        {
            var employee = await _attendanceDAL.GetEmployeeWithPolicyAsync(employeeId);
            if (employee == null)
            {
                //return new AttendanceStateResponse { Message = "Employee not found." };
                return new AttendanceStateResponse { Message = AttendanceMessages.EmployeeNotFound };
            }

            var policy = await ResolveEffectivePolicyAsync(employee);
            if (policy == null)
            {
                //return new AttendanceStateResponse { Message = "Work policy not configured." };
                return new AttendanceStateResponse { Message = AttendanceMessages.WorkPolicyNotConfigured };
            }

            var now = DateTime.UtcNow;
            var today = GetCurrentAttendanceDate();
            var istZone = GetAttendanceTimeZone();
            var holidayLookup = await BuildHolidayLookupAsync(employee, today.Year);
            var nonWorkingDay = ResolveNonWorkingDayInfo(today, policy, holidayLookup);

            var yesterdayAttendance = await _attendanceDAL.GetTodayAttendanceAsync(employeeId, today.AddDays(-1));
            if (yesterdayAttendance != null && yesterdayAttendance.CheckOutTime == null)
            {
                yesterdayAttendance.Status = AttendanceStatus.MissedCheckOut;
                await _attendanceDAL.UpdateAsync(yesterdayAttendance);
                await _attendanceDAL.SaveChangesAsync();
            }

            var attendance = await _attendanceDAL.GetTodayAttendanceAsync(employeeId, today);
            if (attendance == null)
            {
                return nonWorkingDay.IsHoliday || nonWorkingDay.IsWeekOff
                    ? BuildNonWorkingDayState(nonWorkingDay, policy)
                    : BuildNotCheckedInState(policy);
            }

            var referenceTime = attendance.CheckOutTime ?? now;
            var requiredWorkingMinutes = GetRequiredWorkingMinutes(policy);

            var logs = await _attendanceDAL.GetAttendanceLogsAsync(employeeId, attendance.AttendanceDate, attendance.AttendanceDate.AddDays(1));
            var breakSummary = BuildBreakSummary(logs, referenceTime);

            var workedMinutes = attendance.CheckOutTime != null
                ? (attendance.TotalWorkingMinutes > 0 ? attendance.TotalWorkingMinutes : CalculateWorkedMinutes(attendance.CheckInTime, referenceTime, policy, breakSummary.TotalBreakMinutes))
                : CalculateWorkedMinutes(attendance.CheckInTime, referenceTime, policy, breakSummary.TotalBreakMinutes);

            var remainingWorkingMinutes = Math.Max(requiredWorkingMinutes - workedMinutes, 0);
            var isAlmostComplete = remainingWorkingMinutes <= AlmostCompleteThresholdMinutes;

            var checkInIst = attendance.CheckInTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckInTime.Value, istZone) : (DateTime?)null;
            var checkOutIst = attendance.CheckOutTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckOutTime.Value, istZone) : (DateTime?)null;

            return new AttendanceStateResponse
            {
                IsCheckedIn = true,
                IsCheckedOut = attendance.CheckOutTime != null,
                IsLate = attendance.Status == AttendanceStatus.Late,
                IsPaused = breakSummary.IsPaused,
                CanPause = CanPause(policy, breakSummary, remainingWorkingMinutes),
                CanResume = breakSummary.IsPaused,
                RequiredWorkingMinutes = requiredWorkingMinutes,
                WorkedMinutes = workedMinutes,
                RemainingWorkingMinutes = remainingWorkingMinutes,
                MaxBreakMinutes = GetAllowedBreakMinutes(policy),
                BreakMinutesUsed = breakSummary.TotalBreakMinutes,
                BreakMinutesRemaining = GetRemainingBreakMinutes(policy, breakSummary.TotalBreakMinutes),
                MaxBreakCount = policy.MaxBreakCount,
                BreakCountUsed = breakSummary.BreakCountUsed,
                BreakCountRemaining = Math.Max(policy.MaxBreakCount - breakSummary.BreakCountUsed, 0),
                IsBreakPaid = policy.IsBreakPaid,
                IsWorkingHoursAlmostComplete = isAlmostComplete,
                CheckInTime = checkInIst,
                CheckOutTime = checkOutIst,
                Status = ResolveAttendanceStatus(attendance, today),
                IsHoliday = nonWorkingDay.IsHoliday,
                HolidayName = nonWorkingDay.HolidayName,
                IsWeekOff = nonWorkingDay.IsWeekOff,
                IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                WeekOffLabel = nonWorkingDay.WeekOffLabel,
                Message = BuildActiveAttendanceMessage(policy, breakSummary, remainingWorkingMinutes, GetRemainingBreakMinutes(policy, breakSummary.TotalBreakMinutes))
            };
        }

        #endregion

        #region Private Helpers

        private async Task EnsureLocationAllowedAsync(Employee employee, WorkPolicy policy, int employeeId, double lat, double lng)
        {
            var isValidLocation = false;
            if (policy.IsWfoRequired && employee.AssignedOffice != null)
            {
                var office = employee.AssignedOffice;
                var distance = _geoService.CalculateDistanceMeters(lat, lng, (double)office.Latitude!, (double)office.Longitude!);
                if (distance <= office.GeoRadiusMeters) isValidLocation = true;
            }

            if (!isValidLocation && policy.IsWfhAllowed)
            {
                var remote = await _remoteLocationDAL.GetActiveRemoteAsync(employeeId);
                if (remote == null)
                {
                    //await _context.EmployeeRemoteLocations.AddAsync(new EmployeeRemoteLocation { EmployeeId = employeeId, Latitude = (decimal)lat, Longitude = (decimal)lng, GeoRadiusMeters = 200, IsActive = true });
                    await _remoteLocationDAL.AddRemoteLocationAsync(employeeId, lat, lng);

                    isValidLocation = true;
                }
                else
                {
                    var distance = _geoService.CalculateDistanceMeters(lat, lng, (double)remote.Latitude, (double)remote.Longitude);
                    if (distance <= remote.GeoRadiusMeters) isValidLocation = true;
                }
            }

            if (!isValidLocation)
                //throw new Exception("You are not at an allowed location.");
                throw new Exception(AttendanceMessages.InvalidLocation);
        }


        private async Task<(Attendance Attendance, Employee Employee, WorkPolicy Policy)> GetOpenAttendanceContextAsync(int employeeId, DateOnly date)
        {
            var attendance = await _attendanceDAL.GetTodayAttendanceAsync(employeeId,
                                                                          date) ??
                             //throw new Exception("No check-in found.");
                             throw new Exception(AttendanceMessages.NoCheckInFound);

            if (attendance.CheckOutTime != null)
                //throw new Exception("Already checked out.");
                throw new Exception(AttendanceMessages.AlreadyCheckedOut);

            var employee = await _attendanceDAL.GetEmployeeWithPolicyAsync(employeeId);
            if (employee == null)
            {
                //throw new Exception("Employee not found.");
                throw new Exception(AttendanceMessages.EmployeeNotFound);
            }

            var policy = await ResolveEffectivePolicyAsync(employee) ??
                //throw new Exception("Employee or Work Policy not found.");
                throw new Exception(AttendanceMessages.PolicyNotFound);
            return (attendance, employee!, policy);
        }

        private static AttendanceStatus ResolveCheckInStatus(WorkPolicy policy, DateTime checkInUtc)
        {
            if (policy.IsFlexibleShift || policy.ShiftStartTime == null) return AttendanceStatus.Present;
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(checkInUtc, GetAttendanceTimeZone());
            var lateThreshold = policy.ShiftStartTime.Value.Add(TimeSpan.FromMinutes(policy.LateAfterMinutes));
            var nowTime = TimeOnly.FromDateTime(localNow);
            return nowTime > lateThreshold ? AttendanceStatus.Late : AttendanceStatus.Present;
        }

        private static AttendanceStatus ResolveCheckoutStatus(AttendanceStatus currentStatus, WorkPolicy policy, int workedMinutes)
        {
            if (policy.HalfDayThresholdMinutes > 0 && workedMinutes < policy.HalfDayThresholdMinutes) return AttendanceStatus.HalfDay;
            return currentStatus == AttendanceStatus.Late ? AttendanceStatus.Late : AttendanceStatus.Present;
        }

        private static AttendanceStateResponse BuildNotCheckedInState(WorkPolicy policy)
        {
            var allowedBreakMinutes = GetAllowedBreakMinutes(policy);
            var requiredWorkingMinutes = GetRequiredWorkingMinutes(policy);
            return new AttendanceStateResponse
            {
                IsCheckedIn = false,
                RequiredWorkingMinutes = requiredWorkingMinutes,
                RemainingWorkingMinutes = requiredWorkingMinutes,
                MaxBreakMinutes = allowedBreakMinutes,
                BreakMinutesRemaining = allowedBreakMinutes,
                MaxBreakCount = policy.MaxBreakCount,
                BreakCountRemaining = policy.MaxBreakCount,
                Status = string.Empty,
                Message = "Not checked in today."
            };
        }

        private static AttendanceStateResponse BuildNonWorkingDayState(NonWorkingDayInfo nonWorkingDay, WorkPolicy policy)
        {
            var status = nonWorkingDay.IsHoliday ? HolidayStatusLabel : WeekOffStatusLabel;
            var message = nonWorkingDay.IsHoliday
                ? $"Today is {nonWorkingDay.HolidayName}."
                : $"Today is {nonWorkingDay.WeekOffLabel}.";
            var allowedBreakMinutes = GetAllowedBreakMinutes(policy);

            return new AttendanceStateResponse
            {
                Status = status,
                Message = message,
                MaxBreakMinutes = allowedBreakMinutes,
                BreakMinutesRemaining = allowedBreakMinutes,
                MaxBreakCount = policy.MaxBreakCount,
                BreakCountRemaining = policy.MaxBreakCount,
                IsBreakPaid = policy.IsBreakPaid,
                IsHoliday = nonWorkingDay.IsHoliday,
                HolidayName = nonWorkingDay.HolidayName,
                IsWeekOff = nonWorkingDay.IsWeekOff,
                IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                WeekOffLabel = nonWorkingDay.WeekOffLabel
            };
        }


        private static AttendanceLog CreateAttendanceLog(int employeeId, int organizationId, AttendanceLogType logType, DateTime timestamp, double lat, double lng, string ip, string? device)
        {
            return new AttendanceLog
            {
                EmployeeId = employeeId,
                OrganizationId = organizationId,
                LogTimestamp = timestamp,
                LogType = (int)logType,
                Latitude = (decimal)lat,
                Longitude = (decimal)lng,
                IpAddress = ip,
                DeviceInfo = device,
                IsProcessed = true,
                CreatedAt = timestamp
            };
        }


        private static BreakSummary BuildBreakSummary(IEnumerable<AttendanceLog> logs, DateTime referenceTime)
        {
            DateTime? currentPauseStart = null;
            var completedBreakMinutes = 0;
            var breakCountUsed = 0;

            foreach (var log in logs.OrderBy(l => l.LogTimestamp))
            {
                var logType = (AttendanceLogType)log.LogType;
                switch (logType)
                {
                    case AttendanceLogType.Pause when currentPauseStart == null:
                        currentPauseStart = log.LogTimestamp;
                        breakCountUsed++;
                        break;
                    case AttendanceLogType.Resume when currentPauseStart != null:
                        completedBreakMinutes += GetDurationMinutes(currentPauseStart.Value, log.LogTimestamp);
                        currentPauseStart = null;
                        break;
                    case AttendanceLogType.CheckOut when currentPauseStart != null:
                        completedBreakMinutes += GetDurationMinutes(currentPauseStart.Value, log.LogTimestamp);
                        currentPauseStart = null;
                        break;
                }
            }

            var ongoingBreakMinutes = currentPauseStart.HasValue ? GetDurationMinutes(currentPauseStart.Value, referenceTime) : 0;
            return new BreakSummary(currentPauseStart != null, breakCountUsed, completedBreakMinutes, ongoingBreakMinutes);
        }

        private static bool CanPause(WorkPolicy policy, BreakSummary breakSummary, int remainingWorkingMinutes)
        {
            if (breakSummary.IsPaused || remainingWorkingMinutes <= 0) return false;
            var allowedBreakMinutes = GetAllowedBreakMinutes(policy);
            if (policy.MaxBreakCount <= 0 || allowedBreakMinutes <= 0) return false;
            return breakSummary.BreakCountUsed < policy.MaxBreakCount && breakSummary.TotalBreakMinutes < allowedBreakMinutes;
        }

        private static void EnsurePauseAllowed(WorkPolicy policy, BreakSummary breakSummary, int remainingWorkingMinutes)
        {
            if (breakSummary.IsPaused) throw new Exception("Attendance is already paused.");
            if (remainingWorkingMinutes <= 0) throw new Exception("Working hours are already complete.");
            var allowedBreakMinutes = GetAllowedBreakMinutes(policy);
            if (policy.MaxBreakCount <= 0 || allowedBreakMinutes <= 0) throw new Exception("Pause is not enabled.");
            if (breakSummary.BreakCountUsed >= policy.MaxBreakCount) throw new Exception("Break count limit reached.");
            if (breakSummary.TotalBreakMinutes >= allowedBreakMinutes) throw new Exception("Break time limit reached.");
        }

        private static int CalculateWorkedMinutes(DateTime? checkInTime, DateTime endTime, WorkPolicy policy, int breakMinutesUsed)
        {
            if (!checkInTime.HasValue || endTime <= checkInTime.Value) return 0;
            var totalMinutes = GetDurationMinutes(checkInTime.Value, endTime);
            return policy.IsBreakPaid ? totalMinutes : Math.Max(totalMinutes - breakMinutesUsed, 0);
        }

        private static string BuildActiveAttendanceMessage(WorkPolicy policy, BreakSummary breakSummary, int remainingWork, int remainingBreak)
        {
            if (breakSummary.IsPaused) return remainingBreak > 0 ? $"Paused. {remainingBreak} break mins left, {remainingWork} work mins left." : "Paused. Break limit reached.";
            return remainingWork > 0 ? $"You have {remainingWork} minutes remaining." : "Working hours completed.";
        }

        private static int GetAllowedBreakMinutes(WorkPolicy policy)
        {
            var scheduledBreakMinutes = GetScheduledBreakMinutes(policy.BreakStartTime, policy.BreakEndTime);
            var derivedBreakMinutes = scheduledBreakMinutes + Math.Max(policy.AdditionalBreakMinutes, 0);
            return derivedBreakMinutes > 0 ? derivedBreakMinutes : Math.Max(policy.MaxBreakMinutes, 0);
        }

        private static int GetRequiredWorkingMinutes(WorkPolicy policy)
        {
            if (policy.IsFlexibleShift || policy.ShiftStartTime == null || policy.ShiftEndTime == null)
                return policy.RequiredWorkingMinutes;

            if (policy.ShiftEndTime <= policy.ShiftStartTime)
                return policy.RequiredWorkingMinutes;

            var shiftMinutes = (int)(policy.ShiftEndTime.Value - policy.ShiftStartTime.Value).TotalMinutes;
            var scheduledBreakMinutes = GetScheduledBreakMinutes(policy.BreakStartTime, policy.BreakEndTime);
            return policy.IsBreakPaid ? shiftMinutes : Math.Max(shiftMinutes - scheduledBreakMinutes, 1);
        }
        public async Task<List<Employee>> GetEmployeesByOrganizationAsync(int organizationId)
        {
            //return await _context.Employees
            //    .Include(e => e.Department)
            //    .Include(e => e.Designation)
            //    .Where(e => e.OrganizationId == organizationId)
            //    .ToListAsync();
            return await _attendanceDAL.GetEmployeesByOrganizationAsync(organizationId);


        }

        public async Task<List<Attendance>> GetTodayAttendanceForOrganizationAsync(int organizationId, DateOnly date)
        {
            //return await _context.Attendances
            //    .Where(a => a.OrganizationId == organizationId &&
            //                a.AttendanceDate == date)
            //    .ToListAsync();
            return await _attendanceDAL.GetTodayAttendanceForOrganizationAsync(organizationId, date);
        }


        public async Task<List<AttendanceLog>> GetLogsByAttendanceAsync(int employeeId, DateOnly date)
        {
            //var logs = await _context.AttendanceLogs
            //    .Where(l => l.EmployeeId == employeeId &&
            //                DateOnly.FromDateTime(l.LogTimestamp) == date)
            //    .OrderBy(l => l.LogTimestamp)
            //    .ToListAsync();

            //var istZone = GetAttendanceTimeZone();

            //foreach (var log in logs)
            //{
            //    log.LogTimestamp = TimeZoneInfo.ConvertTimeFromUtc(log.LogTimestamp, istZone);
            //}

            //return logs;
            return await _attendanceDAL.GetLogsByAttendanceAsync(employeeId, date);
        }

        public async Task<List<AttendanceLog>> GetLogsWithISTAsync(int employeeId, DateOnly date)
        {
            var logs = await _attendanceDAL
                .GetAttendanceLogsAsync(employeeId, date, date.AddDays(1));

            var istZone = GetAttendanceTimeZone();

            foreach (var log in logs)
            {
                log.LogTimestamp = TimeZoneInfo.ConvertTimeFromUtc(log.LogTimestamp, istZone);
            }

            return logs;
        }

        private static int GetRemainingBreakMinutes(WorkPolicy policy, int breakMinutesUsed)
        {
            return Math.Max(GetAllowedBreakMinutes(policy) - breakMinutesUsed, 0);
        }

        private async Task<WorkPolicy?> ResolveEffectivePolicyAsync(Employee employee)
        {
            var designationIds = employee.DesignationId.HasValue
                ? new[] { employee.DesignationId.Value }
                : Array.Empty<int>();

            var policyCache = await BuildPolicyResolutionCacheAsync(employee.OrganizationId, designationIds);
            return ResolveEffectivePolicy(employee, policyCache);
        }

        private async Task<PolicyResolutionCache> BuildPolicyResolutionCacheAsync(int organizationId, IEnumerable<int> designationIds)
        {
            //var policies = await _context.WorkPolicies
            //    .Where(policy => policy.OrganizationId == organizationId && policy.IsActive)
            //    .Include(policy => policy.WeekOffs)
            //    .AsNoTracking()
            //    .ToListAsync();

            //var designationIdList = designationIds
            //    .Distinct()
            //    .ToList();

            //Dictionary<int, int> designationPolicyIds;
            //if (designationIdList.Count == 0)
            //{
            //    designationPolicyIds = new Dictionary<int, int>();
            //}
            //else
            //{
            //    designationPolicyIds = await _context.DesignationWorkPolicyMappings
            //        .Where(mapping => designationIdList.Contains(mapping.DesignationId))
            //        .ToDictionaryAsync(mapping => mapping.DesignationId, mapping => mapping.WorkPolicyId);
            //}

            //var policiesById = policies.ToDictionary(policy => policy.PolicyId);
            //var defaultPolicy = policies
            //    .Where(policy => policy.IsDefault)
            //    .OrderByDescending(policy => policy.CreatedAt)
            //    .FirstOrDefault();

            //return new PolicyResolutionCache(policiesById, designationPolicyIds, defaultPolicy);
            var policies = await _attendanceDAL.GetActiveWorkPoliciesAsync(organizationId);
            var designationPolicyIds = await _attendanceDAL.GetDesignationPolicyMappingsAsync(designationIds);

            var policiesById = policies.ToDictionary(policy => policy.PolicyId);
            var defaultPolicy = policies.Where(policy => policy.IsDefault).OrderByDescending(policy => policy.CreatedAt).FirstOrDefault();

            return new PolicyResolutionCache(policiesById, designationPolicyIds, defaultPolicy);
        }


        private static WorkPolicy? ResolveEffectivePolicy(Employee employee, PolicyResolutionCache policyCache)
        {
            if (employee.PolicyId.HasValue &&
                policyCache.PoliciesById.TryGetValue(employee.PolicyId.Value, out var employeePolicy))
            {
                return employeePolicy;
            }

            if (employee.DesignationId.HasValue &&
                policyCache.DesignationPolicyIds.TryGetValue(employee.DesignationId.Value, out var designationPolicyId) &&
                policyCache.PoliciesById.TryGetValue(designationPolicyId, out var designationPolicy))
            {
                return designationPolicy;
            }

            return policyCache.DefaultPolicy;
        }

        private async Task<IReadOnlyDictionary<DateOnly, string>> BuildHolidayLookupAsync(
             Employee employee, int year)
        {
            var allGroups = await _holidayDAL.GetActiveGroupsWithDetailsAsync(employee.OrganizationId, year);
            var (resolvedGroup, _) = HolidayResolutionHelper.ResolveGroup(employee, allGroups);
            return BuildHolidayLookupFromGroup(resolvedGroup, year);
        }
        private static IReadOnlyDictionary<DateOnly, string> BuildHolidayLookupForEmployee(
            Employee employee, int year, List<HolidayGroup> allGroups)
        {
            var (resolvedGroup, _) = HolidayResolutionHelper.ResolveGroup(employee, allGroups);
            return BuildHolidayLookupFromGroup(resolvedGroup, year);
        }



        private static Dictionary<DateOnly, string> BuildHolidayLookupFromGroup(HolidayGroup? group, int year)
        {
            if (group == null || !group.IsActive || group.Year != year)
            {
                return new Dictionary<DateOnly, string>();
            }

            return group.GroupHolidays
                .Where(mapping => mapping.SystemHoliday != null)
                .GroupBy(mapping => mapping.SystemHoliday.HolidayDate)
                .ToDictionary(
                    group => group.Key,
                    group => string.Join(", ", group
                        .Select(mapping => mapping.SystemHoliday.HolidayName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct()));
        }

        private static Dictionary<DateOnly, ApprovedLeaveDayInfo> BuildLeaveLookup(IEnumerable<LeaveRequest> leaves, DateOnly startDate, DateOnly endDate)
        {
            var leaveLookup = new Dictionary<DateOnly, ApprovedLeaveDayInfo>();

            foreach (var leave in leaves)
            {
                var dayStart = leave.FromDate > startDate ? leave.FromDate : startDate;
                var dayEnd = leave.ToDate < endDate ? leave.ToDate : endDate;

                for (var date = dayStart; date <= dayEnd; date = date.AddDays(1))
                {
                    if (!leaveLookup.ContainsKey(date))
                    {
                        leaveLookup[date] = new ApprovedLeaveDayInfo(
                            leave.LeaveType?.LeaveTypeName,
                            BuildApproverName(leave.ApprovedBy));
                    }
                }
            }

            return leaveLookup;
        }

        private static NonWorkingDayInfo ResolveNonWorkingDayInfo(
            DateOnly date,
            WorkPolicy? policy,
            IReadOnlyDictionary<DateOnly, string> holidayLookup)
        {
            var isHoliday = holidayLookup.TryGetValue(date, out var holidayName);
            var weekOff = policy?.WeekOffs.FirstOrDefault(item => item.DayOfWeek == date.DayOfWeek);

            return new NonWorkingDayInfo(
                IsHoliday: isHoliday,
                HolidayName: isHoliday ? holidayName : null,
                IsWeekOff: weekOff != null,
                IsHalfDayWeekOff: weekOff?.IsHalfDay ?? false,
                WeekOffLabel: weekOff != null ? BuildWeekOffLabel(weekOff.DayOfWeek, weekOff.IsHalfDay) : null);
        }

        private static string ResolveDayStatus(
            DateOnly date,
            DateOnly today,
            Attendance? attendance,
            ApprovedLeaveDayInfo? leave,
            NonWorkingDayInfo nonWorkingDay)
        {
            if (leave != null)
            {
                return OnLeaveStatusLabel;
            }

            if (attendance != null)
            {
                return ResolveAttendanceStatus(attendance, today);
            }

            if (nonWorkingDay.IsHoliday)
            {
                return HolidayStatusLabel;
            }

            if (nonWorkingDay.IsWeekOff)
            {
                return WeekOffStatusLabel;
            }

            return date < today ? AttendanceStatus.Absent.ToString() : string.Empty;
        }

        private static string ResolveAttendanceStatus(Attendance attendance, DateOnly today)
        {

            if (attendance.CheckInTime == null)
            {
                return AttendanceStatus.Absent.ToString();
            }


            if (attendance.CheckOutTime == null)
            {

                if (attendance.AttendanceDate == today)
                {
                    return "Working";
                }


                return AttendanceStatus.Absent.ToString();
            }


            return attendance.Status.ToString();
        }

        private static string ResolveAdminAttendanceStatus(Attendance attendance)
        {
            return attendance.CheckInTime == null
                ? AttendanceStatus.Absent.ToString()
                : attendance.Status.ToString();
        }

        private static string BuildWeekOffLabel(DayOfWeek dayOfWeek, bool isHalfDay)
        {
            return isHalfDay ? $"{dayOfWeek} (Half Day)" : dayOfWeek.ToString();
        }

        private static string? BuildEmployeeName(Employee? employee)
        {
            if (employee == null)
            {
                return null;
            }

            var nameParts = new[] { employee.FirstName, employee.LastName }
                .Where(part => !string.IsNullOrWhiteSpace(part));

            var fullName = string.Join(" ", nameParts);
            return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
        }

        private static string BuildApproverName(Employee? employee)
        {
            return BuildEmployeeName(employee) ?? "Manager";
        }

        public async Task<AdminAttendanceSummaryResponse> GetTodaySummaryAsync(int organizationId)
        {
            var today = GetCurrentAttendanceDate();

            var employees = await _attendanceDAL.GetEmployeesByOrganizationAsync(organizationId);
            var attendances = await _attendanceDAL.GetTodayAttendanceForOrganizationAsync(organizationId, today);
            //var leaves = await _context.LeaveRequests
            //    .Where(leave => leave.OrganizationId == organizationId &&
            //                    leave.Status == (int)LeaveStatus.Approved &&
            //                    leave.FromDate <= today &&
            //                    leave.ToDate >= today)
            //    .ToListAsync();
            var leaves = await _attendanceDAL.GetApprovedLeavesByOrganizationAsync(organizationId, today);

            var leaveEmpIds = leaves.Select(leave => leave.EmployeeId).ToHashSet();
            var attendanceByEmployeeId = attendances.ToDictionary(attendance => attendance.EmployeeId);
            var policyCache = await BuildPolicyResolutionCacheAsync(
                organizationId,
                employees.Where(employee => employee.DesignationId.HasValue)
                    .Select(employee => employee.DesignationId!.Value));

            int total = employees.Count;
            int present = 0;
            int absent = 0;
            int late = 0;
            int working = 0;
            int leaveCount = leaveEmpIds.Count;
            int holidayCount = 0;
            int weekOffCount = 0;
            var allHolidayGroups = await _holidayDAL.GetActiveGroupsWithDetailsAsync(organizationId, today.Year);

            foreach (var emp in employees)
            {
                if (leaveEmpIds.Contains(emp.EmployeeId))
                {
                    continue;
                }

                attendanceByEmployeeId.TryGetValue(emp.EmployeeId, out var attendance);
                var policy = ResolveEffectivePolicy(emp, policyCache);
                var nonWorkingDay = ResolveNonWorkingDayInfo(
                 today,
                 policy,
                 BuildHolidayLookupForEmployee(emp, today.Year, allHolidayGroups));

                if (attendance == null || attendance.CheckInTime == null)
                {
                    if (nonWorkingDay.IsHoliday)
                    {
                        holidayCount++;
                        continue;
                    }

                    if (nonWorkingDay.IsWeekOff)
                    {
                        weekOffCount++;
                        continue;
                    }

                    absent++;
                    continue;
                }

                present++;

                if (attendance.CheckOutTime == null)
                {
                    working++;
                }

                if (attendance.Status == AttendanceStatus.Late)
                {
                    late++;
                }
            }

            return new AdminAttendanceSummaryResponse
            {
                Total = total,
                Present = present,
                Absent = absent,
                Late = late,
                Working = working,
                Leave = leaveCount,
                Holiday = holidayCount,
                WeekOff = weekOffCount
            };
        }
        private static int GetScheduledBreakMinutes(TimeOnly? breakStart, TimeOnly? breakEnd)
        {
            if (!breakStart.HasValue || !breakEnd.HasValue)
                return 0;
            return (int)(breakEnd.Value - breakStart.Value).TotalMinutes;
        }

        private static int GetDurationMinutes(DateTime start, DateTime end) => Math.Max((int)Math.Floor((end - start).TotalMinutes), 0);

        private static DateOnly GetCurrentAttendanceDate()
        {
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetAttendanceTimeZone());
            return DateOnly.FromDateTime(localNow);
        }

        private static (DateTime FromUtc, DateTime ToUtc) GetAttendanceDateRangeUtc(DateOnly fromDate, DateOnly toDate)
        {
            var timeZone = GetAttendanceTimeZone();
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromDate.ToDateTime(TimeOnly.MinValue), timeZone);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(toDate.ToDateTime(TimeOnly.MinValue), timeZone);
            return (fromUtc, toUtc);
        }

        private static TimeZoneInfo GetAttendanceTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"); }
        }

        public sealed record PolicyResolutionCache(
                IReadOnlyDictionary<int, WorkPolicy> PoliciesById,
                IReadOnlyDictionary<int, int> DesignationPolicyIds,
                WorkPolicy? DefaultPolicy);

        public sealed record NonWorkingDayInfo(
            bool IsHoliday,
            string? HolidayName,
            bool IsWeekOff,
            bool IsHalfDayWeekOff,
            string? WeekOffLabel);

        public sealed record ApprovedLeaveDayInfo(string? LeaveTypeName, string? ApprovedBy);

        public sealed record BreakSummary(bool IsPaused, int BreakCountUsed, int CompletedBreakMinutes, int OngoingBreakMinutes)
        {
            public int TotalBreakMinutes => CompletedBreakMinutes + OngoingBreakMinutes;
        }

        public async Task<List<AdminAttendanceResponse>> GetTodayAttendanceForAdminAsync(int organizationId)
        {
            var today = GetCurrentAttendanceDate();
            var istZone = GetAttendanceTimeZone();

            var employees = await _attendanceDAL.GetEmployeesByOrganizationAsync(organizationId);
            var attendances = await _attendanceDAL.GetTodayAttendanceForOrganizationAsync(organizationId, today);

            var attendanceByEmployeeId = attendances.ToDictionary(attendance => attendance.EmployeeId);

            //var leaves = await _context.LeaveRequests
            //    .Where(leave => leave.OrganizationId == organizationId &&
            //                    leave.Status == (int)LeaveStatus.Approved &&
            //                    leave.FromDate <= today &&
            //                    leave.ToDate >= today)
            //    .Include(leave => leave.LeaveType)
            //    .Include(leave => leave.ApprovedBy)
            //    .ToListAsync();
            var leaves = await _attendanceDAL.GetApprovedLeavesByOrganizationAsync(organizationId, today);


            var leaveMap = leaves
                .GroupBy(leave => leave.EmployeeId)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(leave => leave.FromDate).First());
            var policyCache = await BuildPolicyResolutionCacheAsync(
                organizationId,
                employees.Where(employee => employee.DesignationId.HasValue)
                    .Select(employee => employee.DesignationId!.Value));

            var (fromUtc, toUtc) = GetAttendanceDateRangeUtc(today, today.AddDays(1));
            //var attendanceLogs = await _context.AttendanceLogs
            //    .Where(log => log.OrganizationId == organizationId &&
            //                  log.LogTimestamp >= fromUtc &&
            //                  log.LogTimestamp < toUtc)
            //    .OrderBy(log => log.LogTimestamp)
            //    .ToListAsync();
            var attendanceLogs = await GetAttendanceLogsBYOrganisation(organizationId, fromUtc, toUtc);

            var lastLogByEmployee = attendanceLogs
                        .GroupBy(log => log.EmployeeId)
                        .ToDictionary(group => group.Key, group => group.Last());

            var result = new List<AdminAttendanceResponse>();
            var allHolidayGroups = await _holidayDAL.GetActiveGroupsWithDetailsAsync(organizationId, today.Year);


            foreach (var emp in employees)
            {
                var employeeName = BuildEmployeeName(emp) ?? string.Empty;
                var departmentName = emp.Department?.DepartmentName ?? string.Empty;
                var policy = ResolveEffectivePolicy(emp, policyCache);
                var nonWorkingDay = ResolveNonWorkingDayInfo(
                    today,
                    policy,
                    BuildHolidayLookupForEmployee(emp, today.Year, allHolidayGroups));

                if (leaveMap.TryGetValue(emp.EmployeeId, out var leave))
                {
                    result.Add(new AdminAttendanceResponse
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = employeeName,

                        Status = OnLeaveStatusLabel,
                        IsOnLeave = true,

                        LeaveType = leave.LeaveType?.LeaveTypeName,
                        Reason = leave.Reason,
                        ApprovedBy = BuildApproverName(leave.ApprovedBy),
                        FromDate = leave.FromDate,
                        ToDate = leave.ToDate,
                        Department = departmentName,
                        IsHoliday = nonWorkingDay.IsHoliday,
                        HolidayName = nonWorkingDay.HolidayName,
                        IsWeekOff = nonWorkingDay.IsWeekOff,
                        IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                        WeekOffLabel = nonWorkingDay.WeekOffLabel
                    });

                    continue;
                }

                attendanceByEmployeeId.TryGetValue(emp.EmployeeId, out var attendance);
                string status = AttendanceStatus.Absent.ToString();
                bool paused = false;
                bool isWorking = false;

                if (attendance != null)
                {
                    status = ResolveAdminAttendanceStatus(attendance);

                    if (attendance.CheckInTime != null && attendance.CheckOutTime == null)
                    {
                        isWorking = true;
                    }

                    if (lastLogByEmployee.TryGetValue(emp.EmployeeId, out var lastLog) &&
                        (AttendanceLogType)lastLog.LogType == AttendanceLogType.Pause)
                    {
                        paused = true;
                        isWorking = false;
                    }
                }
                else if (nonWorkingDay.IsHoliday)
                {
                    status = HolidayStatusLabel;
                }
                else if (nonWorkingDay.IsWeekOff)
                {
                    status = WeekOffStatusLabel;
                }


                result.Add(new AdminAttendanceResponse
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = employeeName,

                    CheckInTime = attendance?.CheckInTime.HasValue == true
                        ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckInTime.Value, istZone)
                        : null,
                    CheckOutTime = attendance?.CheckOutTime.HasValue == true
                        ? TimeZoneInfo.ConvertTimeFromUtc(attendance.CheckOutTime.Value, istZone)
                        : null,

                    Status = status,
                    IsWorking = isWorking,
                    IsPaused = paused,

                    IsOnLeave = false,
                    Department = departmentName,
                    IsHoliday = nonWorkingDay.IsHoliday,
                    HolidayName = nonWorkingDay.HolidayName,
                    IsWeekOff = nonWorkingDay.IsWeekOff,
                    IsHalfDayWeekOff = nonWorkingDay.IsHalfDayWeekOff,
                    WeekOffLabel = nonWorkingDay.WeekOffLabel
                });
            }

            return result;
        }

        private async Task<List<AttendanceLog>> GetAttendanceLogsBYOrganisation(int organizationId, DateTime fromUtc, DateTime toUtc)
        {
            return await _attendanceDAL.GetAttendanceLogsBYOrganisation(organizationId, fromUtc, toUtc);
        }
   
        // submit req 
        public async Task SubmitMissedCheckoutRequest(int employeeId, int attendanceId, string reason)
        {
            //var attendance = await _context.Attendances
            //    .IgnoreQueryFilters()
            //    .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);
            var attendance = await _attendanceDAL.GetAttendanceByIdIgnoreFiltersAsync(attendanceId);


            if (attendance == null)
                throw new Exception($"Attendance not found for ID: {attendanceId}");


            Console.WriteLine($"[DEBUG] UI EmployeeId: {employeeId}");
            Console.WriteLine($"[DEBUG] DB EmployeeId: {attendance.EmployeeId}");


            if (attendance.EmployeeId != employeeId)
                throw new Exception("Unauthorized access: Employee mismatch");


            var today = GetCurrentAttendanceDate();

            var isMissed =
                attendance.AttendanceDate != today &&
                attendance.CheckInTime != null &&
                attendance.CheckOutTime == null;

            if (!isMissed)
                throw new Exception("Invalid request. Only past missed checkouts allowed.");


            var exists = await _attendanceDAL.HasPendingRequest(attendanceId);
            if (exists)
                throw new Exception("Request already submitted");

            var request = new AttendanceRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                AttendanceId = attendanceId,
                Reason = reason,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            await _attendanceDAL.CreateRequestAsync(request);
            await _attendanceDAL.SaveChangesAsync();
        }


        //get all

        public async Task<List<AttendanceRequestResponse>> GetAllAttendanceRequests()
        {
            _permissionService.Ensure(PermissionKeys.ATT_EDIT_LOGS);
            return await _attendanceDAL.GetAllRequestsAsync();
        }

        // approve 

        public async Task ApproveRequest(Guid requestId, DateTime checkoutTime)
        {
            var request = await _attendanceDAL.GetRequestByIdAsync(requestId)
                ?? throw new Exception("Request not found");

            var loggedInUserId = _permissionService.GetCurrentEmployeeId();

            if (loggedInUserId == request.EmployeeId)
            {
                throw new Exception("SELF_APPROVAL_NOT_ALLOWED");
            }


            _permissionService.EnsureCanActOn(PermissionKeys.ATT_EDIT_LOGS, request.EmployeeId);

            if (request.Status != "PENDING")
                throw new Exception("Already processed");

            //var attendance = await _context.Attendances
            //    .FirstOrDefaultAsync(a => a.AttendanceId == request.AttendanceId);
            var attendance = await _attendanceDAL.GetAttendanceByIdAsync(request.AttendanceId);


            if (attendance == null)
                throw new Exception("Attendance not found");


            request.Status = "APPROVED";
            request.ApprovedCheckoutTime = checkoutTime;
            request.UpdatedAt = DateTime.UtcNow;


            attendance.CheckOutTime = checkoutTime;
            attendance.IsManualEntry = true;

            var minutes = (int)(checkoutTime - attendance.CheckInTime!.Value).TotalMinutes;
            attendance.TotalWorkingMinutes = Math.Max(minutes, 0);

            attendance.Status = AttendanceStatus.Present;

            await _attendanceDAL.UpdateAsync(attendance);
            await _attendanceDAL.UpdateRequestAsync(request);
            await _attendanceDAL.SaveChangesAsync();
        }

        public async Task<PagedResult<ShiftMaster>> GetPagedShiftsAsync(int pageNumber, int pageSize)
        {
            var shifts = await _attendanceDAL.GetPagedShiftsAsync(pageNumber, pageSize);

            var totalCount = shifts.Count();

            var items = shifts
                .OrderBy(s => s.ShiftName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ShiftMaster>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // reject

        public async Task RejectRequest(Guid requestId, string remarks)
        {
            AttendanceRequest request = await _attendanceDAL.GetRequestByIdAsync(requestId)
                ?? throw new Exception("Request not found");

            _permissionService.EnsureCanActOn(PermissionKeys.ATT_EDIT_LOGS, request.EmployeeId);

            if (request.Status != "PENDING")
                throw new Exception("Already processed");

            request.Status = "REJECTED";
            request.AdminRemarks = remarks;
            request.UpdatedAt = DateTime.UtcNow;

            await _attendanceDAL.UpdateRequestAsync(request);
            await _attendanceDAL.SaveChangesAsync();
        }

        #endregion
    }

    public enum AttendanceLogType
    {
        CheckIn = 1,
        CheckOut = 2,
        Pause = 3,
        Resume = 4,
        Void = 5
    }
}