public class SalaryStructureModel
{
    public string StructureName { get; set; } = null!;

    
    public string PayType { get; set; } = "Monthly"; 
    // Values: Monthly / Yearly / Daily

    public bool IsDefault { get; set; } = false;

    public List<SalaryComponentModel> Components { get; set; } = new();
}