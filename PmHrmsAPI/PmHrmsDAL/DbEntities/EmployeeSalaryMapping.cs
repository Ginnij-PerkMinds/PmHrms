namespace PmHrmsAPI.PmHrmsDAL.DbEntities;
public class EmployeeSalaryMapping
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public int SalaryStructureId { get; set; }

    public int OrganizationId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee Employee { get; set; } = null!;
    public SalaryStructure SalaryStructure { get; set; } = null!;
}