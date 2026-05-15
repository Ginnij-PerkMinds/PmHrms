using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;
namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class PayrollRun
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public byte PayrollMonth { get; set; }
    public short PayrollYear { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public PayrollRunType RunType { get; set; } 
        public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public int? LockedBy { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = string.Empty;
    public DateOnly? ScheduledRunDate { get; set; }
    public DateTime? ActualRunStart { get; set; }
    public DateTime? ActualRunEnd { get; set; }
    public int? TotalEmployees { get; set; }
    public decimal? TotalNetPayable { get; set; }
    public bool IsRecalculated { get; set; }
    public int? ParentPayrollRunId { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public PayrollRun? ParentPayrollRun { get; set; }
    public ICollection<PayrollRun> ReRuns { get; set; } = [];
    public ICollection<EmployeePayroll> EmployeePayrolls { get; set; } = [];
    public ICollection<EmployeePayrollHistory> PayrollHistories { get; set; } = [];
    public ICollection<PayrollAuditLog> AuditLogs { get; set; } = [];
    public ICollection<SalaryRevisionLog> SalaryRevisions { get; set; } = [];
}
