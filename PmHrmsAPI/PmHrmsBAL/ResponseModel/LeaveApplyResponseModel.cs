namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class LeaveApplyResponseModel
    {
        public int LeaveId { get; set; }
        public int LeaveTypeId { get; set; }
        public string Reason { get; set; }
        public decimal TotalDays { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
