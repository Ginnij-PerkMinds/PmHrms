namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class EmployeePayrollComponent
{
    public int Id { get; set; }
    public int EmployeePayrollId { get; set; }
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsStatutory { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? CalculationBasis { get; set; }
    public decimal? BaseAmount { get; set; }
    public decimal? Rate { get; set; }
    public string? FormulaSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }

    public required EmployeePayroll EmployeePayroll { get; set; }
}
