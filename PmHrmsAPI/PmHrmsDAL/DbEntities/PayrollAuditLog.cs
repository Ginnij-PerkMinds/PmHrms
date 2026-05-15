namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class PayrollAuditLog
{
    public long Id { get; set; }
    public int PayrollRunId { get; set; }
    public int? EmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? IpAddress { get; set; }

    public required PayrollRun PayrollRun { get; set; }
}
