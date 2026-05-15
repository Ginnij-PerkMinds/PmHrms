public class SalaryStructure
{
    public int SalaryStructureId { get; set; }
    public int OrganizationId { get; set; }
    public string StructureName { get; set; } = null!;
    
    // Yearly/Monthly/Daily switch
    public string PayType { get; set; } = "Monthly"; 

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalaryComponent> Components { get; set; } = new List<SalaryComponent>();
}

