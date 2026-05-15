 namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
 public class LeaveTypeResponseModel
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = null!;
        public int? MaxDaysPerApplication { get; set; }
        public bool IsBalanceBased { get; set; }
        public bool IsSpecialPolicy { get; set; }
        public bool IsActive { get; set; }
    }
}