using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class WorkPolicy
{
    public int PolicyId { get; set; }

    public string PolicyName { get; set; } = null!;

    public int RequiredWorkingMinutes { get; set; }

    public int LateAfterMinutes { get; set; }

    public int HalfDayThresholdMinutes { get; set; }

    public bool IsWfhAllowed { get; set; }

    public bool IsWfoRequired { get; set; }

    public int OrganizationId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public TimeOnly? ShiftStartTime { get; set; }

    public TimeOnly? ShiftEndTime { get; set; }

    public bool IsFlexibleShift { get; set; }

    public int MaxBreakMinutes { get; set; }

    public int MaxBreakCount { get; set; }

    public bool IsBreakPaid { get; set; }

    public TimeOnly? BreakStartTime { get; set; }

    public TimeOnly? BreakEndTime { get; set; }

    public int AdditionalBreakMinutes { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<WorkPolicyWeekOff> WeekOffs { get; set; } = new List<WorkPolicyWeekOff>();
}
