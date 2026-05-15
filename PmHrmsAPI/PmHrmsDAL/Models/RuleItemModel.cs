namespace PmHrmsAPI.PmHrmsDAL.Models
{ public class RuleItemModel
    {
        public int     LeaveTypeId  { get; set; }
        public int     DaysPerMonth { get; set; }
        public bool    CarryForward { get; set; }
    }
}