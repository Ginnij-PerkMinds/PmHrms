namespace PmHrmsAPI.PmHrmsBAL.Models;

public sealed class PayrollContext
{
    public int EmployeeId { get; init; }
    public decimal GrossSalary { get; init; }
    public byte TotalWorkingDays { get; init; }
    public decimal PresentDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }
    public bool IsPfApplicable { get; init; }
    public decimal? PfWageLimit { get; init; }
    public decimal? PfEmployeePercentage { get; init; }
    public decimal? PfEmployerPercentage { get; init; }
    public bool IsEsiApplicable { get; init; }
    public decimal? EsiEmployeePercentage { get; init; }
    public decimal? EsiEmployerPercentage { get; init; }
    public bool IsTdsApplicable { get; init; }
    public decimal? TdsPercentage { get; init; }
    public string RoundingRule { get; init; } = "Round";

      public List<SalaryComponent> SalaryComponents { get; set; } = [];
}
