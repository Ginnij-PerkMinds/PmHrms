namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class EmployeeAttendanceSummary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public byte Month { get; set; }
    public short Year { get; set; }
    public byte TotalWorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public byte HalfDays { get; set; }
    public byte WeekOffs { get; set; }
    public byte Holidays { get; set; }
    public decimal OvertimeHours { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
