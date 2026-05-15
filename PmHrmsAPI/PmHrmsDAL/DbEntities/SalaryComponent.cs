public class SalaryComponent
{
    public int SalaryComponentId { get; set; }
    public int SalaryStructureId { get; set; }
    
     public int OrganizationId { get; set; } 
    
    public int ComponentMasterId { get; set; } 
    public string ComponentName { get; set; } = null!; 
    
    public decimal Amount { get; set; }
    public bool IsEarning { get; set; }


    public SalaryStructure SalaryStructure { get; set; } = null!;
            

    public SalaryComponentMaster SalaryComponentMaster { get; set; } = null!;
}