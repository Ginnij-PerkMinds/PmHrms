namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{public class RuleItemResponseModel
    {
        public int     RuleId       { get; set; }  
        public int     LeaveTypeId  { get; set; }
        public string  LeaveTypeName { get; set; } = null!;
        public int     DaysPerMonth { get; set; }
        public bool    CarryForward { get; set; }
    }
}