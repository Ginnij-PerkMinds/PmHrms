namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class LeaveBalanceResponseModel
    {
        public int LeaveTypeId          { get; set; }
        public string LeaveTypeName     { get; set; } = null!;
        public bool IsBalanceBased      { get; set; }
        public decimal Balance          { get; set; }
        public decimal Used             { get; set; }
        public decimal PreDeducted      { get; set; }
        public decimal CarryForwardBalance { get; set; }   
        public decimal Available => Math.Max(Balance + CarryForwardBalance - Used - PreDeducted, 0);
    }
}