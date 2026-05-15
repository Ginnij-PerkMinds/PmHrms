namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class LeaveMasterResponseModel
    {
        public int LeaveMasterId { get; set; }
        public string LeaveTypeName { get; set; } = null!;
        public int? MaxDaysPerApplication { get; set; }
        public bool IsBalanceBased { get; set; }
        public bool IsSpecialPolicy { get; set; }
    }
}