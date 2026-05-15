namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class PayrollConfig
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public byte AutoRunDayOfMonth { get; set; }
    public bool IsAutoRunEnabled { get; set; }
    public byte PayrollCutoffDay { get; set; }
    public byte PayslipGenerateDay { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string RoundingRule { get; set; } = string.Empty;
    public bool IsPfApplicable { get; set; }
    public decimal? PfWageLimit { get; set; }
    public decimal? PfEmployeePercentage { get; set; }
    public decimal? PfEmployerPercentage { get; set; }
    public bool IsEsiApplicable { get; set; }
    public decimal? EsiEmployeePercentage { get; set; }
    public decimal? EsiEmployerPercentage { get; set; }
    public bool IsTdsApplicable { get; set; }
    public decimal? TdsPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
