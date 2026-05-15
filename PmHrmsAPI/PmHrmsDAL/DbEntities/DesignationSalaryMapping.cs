namespace PmHrmsAPI.PmHrmsDAL.DbEntities;
public class DesignationSalaryMapping
{
    public int Id { get; set; }

    public int DesignationId { get; set; }
    public int SalaryStructureId { get; set; }

    public int OrganizationId { get; set; }

    // Navigation
    public Designation Designation { get; set; } = null!;
    public SalaryStructure SalaryStructure { get; set; } = null!;
}