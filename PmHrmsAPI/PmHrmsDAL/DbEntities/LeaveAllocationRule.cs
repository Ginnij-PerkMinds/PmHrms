using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class LeaveAllocationRule
{
    public int RuleId { get; set; }

    public string RuleName { get; set; } = null!;

    public int LeaveTypeId { get; set; }

    public int DaysPerMonth { get; set; }

    public bool CarryForward { get; set; }

    public bool IsDefault { get; set; }

    public int OrganizationId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LeaveRuleDesignation> LeaveRuleDesignations { get; set; } = new List<LeaveRuleDesignation>();

    public virtual LeaveType LeaveType { get; set; } = null!;
}
