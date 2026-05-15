namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AttendanceCalendarDayResponse
    {
        public int? AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int TotalWorkingMinutes { get; set; }
        public bool IsOnLeave { get; set; }
        public string? LeaveType { get; set; }
        public string? ApprovedBy { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsWeekOff { get; set; }
        public bool IsHalfDayWeekOff { get; set; }
        public string? WeekOffLabel { get; set; }
        public string? RequestStatus { get; set; }
    }
}
