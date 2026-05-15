namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AdminAttendanceResponse 
    {
        public int EmployeeId { get; set; }   

        public string EmployeeName { get; set; }    

        public DateTime? CheckInTime { get; set; }   

        public DateTime? CheckOutTime { get; set; }                

        public string Status { get; set; }

        public bool IsPaused { get; set; }

        public string Department { get; set; }

        public string? LeaveType { get; set; }
        public string? Reason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        public bool IsOnLeave { get; set; }
        public bool IsWorking { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsWeekOff { get; set; }
        public bool IsHalfDayWeekOff { get; set; }
        public string? WeekOffLabel { get; set; }



    }
}
