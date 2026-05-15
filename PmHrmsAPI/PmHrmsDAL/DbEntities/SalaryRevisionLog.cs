namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class SalaryRevisionLog
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public decimal OldGross { get; set; }
    public decimal NewGross { get; set; }
    public int RevisedBy { get; set; }
    public string? Reason { get; set; }
    public int? PayrollRunId { get; set; }
    public DateTime CreatedAt { get; set; }

    public PayrollRun? PayrollRun { get; set; }
}
