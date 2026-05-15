namespace PmHrmsAPI.PmHrmsBAL.ResponseModel;

public class UpdatePayrollConfigRequest
{
    public byte AutoRunDay { get; set; }
    public bool AutoRunEnabled { get; set; }
    public byte PayrollCutoffDay { get; set; }
    public byte PayslipGenerateDay { get; set; }
    public string? Currency { get; set; }
    public string? RoundingRule { get; set; }
    public bool IsPfApplicable { get; set; }
    public decimal? PfWageLimit { get; set; }
    public decimal? PfEmployeePercentage { get; set; }
    public decimal? PfEmployerPercentage { get; set; }
    public bool IsEsiApplicable { get; set; }
    public decimal? EsiEmployeePercentage { get; set; }
    public decimal? EsiEmployerPercentage { get; set; }
    public bool IsTdsApplicable { get; set; }
    public decimal? TdsPercentage { get; set; }
}

public class PayrollConfigResponse
{
    public byte AutoRunDay { get; set; }
    public bool AutoRunEnabled { get; set; }
    public byte PayrollCutoffDay { get; set; }
    public byte PayslipGenerateDay { get; set; }
    public string Currency { get; set; } = "INR";
    public string RoundingRule { get; set; } = "Round";
    public bool IsPfApplicable { get; set; }
    public decimal? PfWageLimit { get; set; }
    public decimal? PfEmployeePercentage { get; set; }
    public decimal? PfEmployerPercentage { get; set; }
    public bool IsEsiApplicable { get; set; }
    public decimal? EsiEmployeePercentage { get; set; }
    public decimal? EsiEmployerPercentage { get; set; }
    public bool IsTdsApplicable { get; set; }
    public decimal? TdsPercentage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "System";
}
