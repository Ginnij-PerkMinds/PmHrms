using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class SpecialLeaveRequest
{
    public int RequestId { get; set; }

    public int LeaveId { get; set; }

    public string RequestedLeaveName { get; set; } = null!;

    public string? Description { get; set; }

    public string? SupportingDocument { get; set; }

    public string? HrComment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LeaveRequest Leave { get; set; } = null!;
}
