using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using System.ComponentModel;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class AttendanceDAL
    {
        private readonly PmHrmsContext _context;

        public AttendanceDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId, DateOnly date)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.EmployeeId == employeeId &&
                    a.AttendanceDate == date);
        }

        public async Task AddAsync(Attendance attendance)
        {
            await _context.Attendances.AddAsync(attendance);
        }

        public Task UpdateAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            return Task.CompletedTask;
        }

        public async Task<List<Employee>> GetEmployeesByOrganizationAsync(int organizationId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Policy)
                    .ThenInclude(p => p!.WeekOffs)
                .Include(e => e.HolidayGroup)
                    .ThenInclude(g => g!.GroupHolidays)
                    .ThenInclude(gh => gh.SystemHoliday)
                .Include(e => e.AssignedOffice)
                .AsSplitQuery()
                .Where(e => e.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetTodayAttendanceForOrganizationAsync(int organizationId, DateOnly date)
        {
            return await _context.Attendances
                .Where(a => a.OrganizationId == organizationId &&
                            a.AttendanceDate == date)
                .ToListAsync();
        }
        public async Task<List<AttendanceLog>> GetLogsByAttendanceAsync(int employeeId, DateTime date)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var start = date.Date;
            var end = date.Date.AddDays(1);

            var logs = await _context.AttendanceLogs
                .Where(l =>
                    l.EmployeeId == employeeId &&
                    l.LogTimestamp >= start &&
                    l.LogTimestamp < end
                )
                .OrderBy(l => l.LogTimestamp)
                .ToListAsync();

            return logs.Select(l => new AttendanceLog
            {
                LogId = l.LogId,
                EmployeeId = l.EmployeeId,
                OrganizationId = l.OrganizationId,

                LogTimestamp = TimeZoneInfo.ConvertTimeFromUtc(l.LogTimestamp, istZone),

                LogType = l.LogType,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                IpAddress = l.IpAddress,
                DeviceInfo = l.DeviceInfo,
                IsProcessed = l.IsProcessed,
                CreatedAt = l.CreatedAt
            }).ToList();
        }

        public async Task<List<AttendanceLog>> GetAttendanceLogsAsync(int employeeId, DateOnly fromDate, DateOnly toDate)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Convert IST → UTC
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(
                fromDate.ToDateTime(TimeOnly.MinValue), istZone);

            var toUtc = TimeZoneInfo.ConvertTimeToUtc(
                toDate.ToDateTime(TimeOnly.MinValue), istZone);

            return await _context.AttendanceLogs
                .Where(l =>
                    l.EmployeeId == employeeId &&
                    l.LogTimestamp >= fromUtc &&
                    l.LogTimestamp < toUtc
                )
                .OrderBy(l => l.LogTimestamp)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeWithPolicyAsync(int employeeId)
        {
            return await _context.Employees
                .Include(e => e.Policy)
                    .ThenInclude(p => p!.WeekOffs)
                .Include(e => e.Designation)
                .Include(e => e.HolidayGroup)
                    .ThenInclude(g => g!.GroupHolidays)
                    .ThenInclude(gh => gh.SystemHoliday)
                .Include(e => e.AssignedOffice)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int employeeId, DateOnly doj, int page, int size)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= doj)
                .OrderByDescending(a => a.AttendanceDate)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }


        public async Task<int> GetAttendanceHistoryCountAsync(int employeeId, DateOnly doj)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= doj)
                .CountAsync();
        }

        public async Task<List<Attendance>> GetAttendanceByDateRangeAsync(int employeeId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId &&
                            a.AttendanceDate >= startDate &&
                            a.AttendanceDate <= endDate)
                .ToListAsync();
        }

        public async Task<List<AttendanceRequest>> GetAttendanceRequestsByEmployeeAsync(int employeeId)
        {
            return await _context.AttendanceRequests
                .Where(r => r.EmployeeId == employeeId &&
                            (r.Status == "PENDING" || r.Status == "APPROVED" || r.Status == "REJECTED"))
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetApprovedLeavesByEmployeeAsync(int employeeId, DateOnly startDate, DateOnly endDate)
        {
            return await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId &&
                            l.Status == (int)LeaveStatus.Approved &&
                            l.FromDate <= endDate &&
                            l.ToDate >= startDate)
                .Include(l => l.LeaveType)
                .Include(l => l.ApprovedBy)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetApprovedLeavesByOrganizationAsync(int organizationId, DateOnly date)
        {
            return await _context.LeaveRequests
                .Where(leave => leave.OrganizationId == organizationId &&
                                leave.Status == (int)LeaveStatus.Approved &&
                                leave.FromDate <= date &&
                                leave.ToDate >= date)
                .Include(leave => leave.LeaveType)
                .Include(leave => leave.ApprovedBy)
                .ToListAsync();
        }

        public async Task<Attendance?> GetAttendanceByIdIgnoreFiltersAsync(int attendanceId)
        {
            return await _context.Attendances
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);
        }


        public async Task<Attendance?> GetAttendanceByIdAsync(int attendanceId)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);
        }


        public async Task<List<AttendanceLog>> GetAttendanceLogsBYOrganisation(int organizationId, DateTime fromUtc, DateTime toUtc)
        {
            
            return await _context.AttendanceLogs
           .Where(l => l.OrganizationId == organizationId &&
                       l.LogTimestamp >= fromUtc &&
                       l.LogTimestamp <= toUtc)
        .OrderBy(l => l.LogTimestamp)
        .ToListAsync();
        }


        public async Task<List<AttendanceRequestResponse>> GetAllRequestsAsync()
        {
            return await _context.AttendanceRequests
                .Select(r => new AttendanceRequestResponse
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    AttendanceId = r.AttendanceId,
                    Reason = r.Reason,
                    Status = r.Status,
                    AdminRemarks = r.AdminRemarks,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    ApprovedCheckoutTime = r.ApprovedCheckoutTime
                })
                .ToListAsync();
        }


        public async Task CreateRequestAsync(AttendanceRequest request)
        {
            await _context.AttendanceRequests.AddAsync(request);
        }

        public async Task<bool> HasPendingRequest(int attendanceId)
        {
            return await _context.AttendanceRequests
                .AnyAsync(r => r.AttendanceId == attendanceId && r.Status == "PENDING");
        }


        public async Task<List<AttendanceLog>> GetLogsByAttendanceAsync(int employeeId, DateOnly date)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), istZone);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue), istZone);

            var logs = await _context.AttendanceLogs
                .Where(l => l.EmployeeId == employeeId &&
                            l.LogTimestamp >= fromUtc &&
                            l.LogTimestamp < toUtc)
                .OrderBy(l => l.LogTimestamp)
                .ToListAsync();

            foreach (var log in logs)
            {
                log.LogTimestamp = TimeZoneInfo.ConvertTimeFromUtc(log.LogTimestamp, istZone);
            }

            return logs;           
        }

        public async Task<List<Employee>> GetEmployeesByOrganizationAsync(int organizationId, bool minimal)
        {
            // overload that allows a minimal projection (keeps backward compatibility if called elsewhere)
            if (minimal)
            {
                return await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Designation)
                    .Where(e => e.OrganizationId == organizationId)
                    .ToListAsync();
            }

            return await GetEmployeesByOrganizationAsync(organizationId);
        }

        public async Task<List<WorkPolicy>> GetActiveWorkPoliciesAsync(int organizationId)
        {
            return await _context.WorkPolicies
                .Where(policy => policy.OrganizationId == organizationId && policy.IsActive)
                .Include(policy => policy.WeekOffs)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetDesignationPolicyMappingsAsync(IEnumerable<int> designationIds)
        {
            return await _context.DesignationWorkPolicyMappings
                .Where(mapping => designationIds.Contains(mapping.DesignationId))
                .ToDictionaryAsync(mapping => mapping.DesignationId, mapping => mapping.WorkPolicyId);
        }

        public async Task AddRemoteLocationAsync(int employeeId, double lat, double lng)
        {
            var remoteLocation = new EmployeeRemoteLocation
            {
                EmployeeId = employeeId,
                Latitude = (decimal)lat,
                Longitude = (decimal)lng,
                GeoRadiusMeters = 200,
                IsActive = true
            };

            await _context.EmployeeRemoteLocations.AddAsync(remoteLocation);
            await _context.SaveChangesAsync();
        }

        // Internal helpers left as NotImplemented - implement as needed.
        internal async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        internal async Task<bool> IsEmployeeOnLeave(int employeeId, DateOnly today)
        {
            // simple implementation: check approved leave overlapping today
            var exists = await _context.LeaveRequests
                .AnyAsync(l => l.EmployeeId == employeeId &&
                               l.Status == (int)LeaveStatus.Approved &&
                               l.FromDate <= today &&
                               l.ToDate >= today);
            return exists;
        }

        internal async Task AddLogAsync(AttendanceLog attendanceLog)
        {
            await _context.AttendanceLogs.AddAsync(attendanceLog);
        }

        internal async Task GetAttendanceLogsByOrganizationAsync(int organizationId, DateTime fromUtc, DateTime toUtc)
        {
            // placeholder to match signature - actual implementation may return data or perform actions
            await _context.AttendanceLogs
                .Where(l => l.OrganizationId == organizationId &&
                            l.LogTimestamp >= fromUtc &&
                            l.LogTimestamp < toUtc)
                .ToListAsync();
        }

        internal async Task UpdateRequestAsync(object request)
        {
            _context.Update(request!);
            await Task.CompletedTask;
        }

        internal async Task<AttendanceRequest?> GetRequestByIdAsync(Guid requestId)
        {
            return await _context.AttendanceRequests.FindAsync(requestId);
        }

        public async Task<List<ShiftMaster>> GetPagedShiftsAsync(int pageNumber, int pageSize)
        {
            return await _context.ShiftMasters
                .OrderBy(s => s.ShiftName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        internal async Task<List<LeaveRequest>> GetTodayApprovedLeaves(int orgId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return await _context.LeaveRequests
                .Where(leave => leave.OrganizationId == orgId &&
                                leave.Status == (int)LeaveStatus.Approved &&
                                leave.FromDate <= today &&
                                leave.ToDate >= today)
                .Include(leave => leave.LeaveType)
                .Include(leave => leave.ApprovedBy)
                .ToListAsync();
        }
    }
}
