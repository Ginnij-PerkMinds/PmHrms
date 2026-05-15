namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AdminAttendanceSummaryResponse
    {
        public int Total { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int Working { get; set; }
        public int Leave { get; set; }
        public int Holiday { get; set; }
        public int WeekOff { get; set; }
    }
}
