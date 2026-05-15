using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class LeaveRequest
{
    public int LeaveId { get; set; }

    public int EmployeeId { get; set; }

    public int OrganizationId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public string? Reason { get; set; }

    public int Status { get; set; }

    public int? ApprovedById { get; set; }

    public DateTime AppliedAt { get; set; }

    public DateTime? ActionAt { get; set; }

    public int? CalendarDays { get; set; }

    public bool IsSpecialRequest { get; set; }

    public virtual LeaveType LeaveType { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedBy { get; set; }
    public virtual ICollection<SpecialLeaveRequest> SpecialLeaveRequests { get; set; } = new List<SpecialLeaveRequest>();
}
