namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AttendanceStateResponse
    {
        public bool IsCheckedIn { get; set; }
        public bool IsCheckedOut { get; set; }
        public bool IsLate { get; set; }
        public bool IsPaused { get; set; }
        public bool CanPause { get; set; }
        public bool CanResume { get; set; }
        public int RequiredWorkingMinutes { get; set; }
        public int WorkedMinutes { get; set; }
        public int RemainingWorkingMinutes { get; set; }
        public int MaxBreakMinutes { get; set; }
        public int BreakMinutesUsed { get; set; }
        public int BreakMinutesRemaining { get; set; }
        public int MaxBreakCount { get; set; }
        public int BreakCountUsed { get; set; }
        public int BreakCountRemaining { get; set; }
        public bool IsBreakPaid { get; set; }
        public bool IsWorkingHoursAlmostComplete { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsWeekOff { get; set; }
        public bool IsHalfDayWeekOff { get; set; }
        public string? WeekOffLabel { get; set; }
    }
}
