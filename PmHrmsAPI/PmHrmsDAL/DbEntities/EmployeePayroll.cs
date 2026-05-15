namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class EmployeePayroll
{
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public byte TotalWorkingDays { get; set; }
    public decimal PresentDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal LopDays { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPayable { get; set; }
    public decimal ArrearsAmount { get; set; }
    public decimal RoundingAdjustment { get; set; }
    public string? SalaryStructureSnapshot { get; set; }
    public string? BankAccountSnapshot { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? PaymentDate { get; set; }
    public string? PaymentMode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public required PayrollRun PayrollRun { get; set; }
    public ICollection<EmployeePayrollComponent> Components { get; set; } = [];
}
