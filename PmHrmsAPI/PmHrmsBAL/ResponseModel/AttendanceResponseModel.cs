namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AttendanceResponseModel
    {
        public int AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalWorkingMinutes { get; set; }
        public bool IsClockedIn { get; set; }
        public string? RequestStatus { get; set; }
        public bool IsOnLeave { get; set; }
        public string? LeaveType { get; set; }
        public string? ApprovedBy { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsWeekOff { get; set; }
        public bool IsHalfDayWeekOff { get; set; }
        public string? WeekOffLabel { get; set; }

        public bool CanRequestCorrection { get; set; }

        public bool IsWorking { get; set; }
    }
}
