using System;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class LeaveBalance
    {
        public int BalanceId { get; set; }

        public int EmployeeId { get; set; }

        public int LeaveTypeId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public decimal Balance { get; set; }

        public decimal Used { get; set; }

        public decimal PreDeducted { get; set; }

        public int? RuleSnapshotId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public Employee Employee { get; set; }

        public LeaveType LeaveType { get; set; }
    }
}