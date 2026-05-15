namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class EmployeePayrollHistory
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public byte PayrollMonth { get; set; }
    public short PayrollYear { get; set; }
    public int PayrollRunId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetPayable { get; set; }
    public string JsonSnapshot { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public required PayrollRun PayrollRun { get; set; }
}
