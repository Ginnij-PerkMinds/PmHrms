namespace PmHrmsAPI.PmHrmsDAL.Models
{
public class LeaveTypeModel
    {
        public string LeaveTypeName { get; set; } = null!;
        public int? MaxDaysPerApplication { get; set; }
        public bool IsBalanceBased { get; set; } = true;
        public bool IsSpecialPolicy { get; set; }
        public int? LeaveMasterId { get; set; }
    }
}