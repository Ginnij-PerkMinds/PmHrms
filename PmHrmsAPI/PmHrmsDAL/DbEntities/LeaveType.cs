using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class LeaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeName { get; set; } = null!;

    public int OrganizationId { get; set; }

    public bool IsActive { get; set; }

    public int? MaxDaysPerApplication { get; set; }

    public bool IsBalanceBased { get; set; }

    public bool IsSpecialPolicy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? LeaveMasterId { get; set; }

    public virtual ICollection<LeaveAllocationRule> LeaveAllocationRules { get; set; } = new List<LeaveAllocationRule>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual LeaveMaster? LeaveMaster { get; set; }
}
