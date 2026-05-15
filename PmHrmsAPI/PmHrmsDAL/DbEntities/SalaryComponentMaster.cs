public class SalaryComponentMaster
{
    public int Id { get; set; }
  
    public string ComponentName { get; set; } = null!; // e.g., "Basic Salary"
    public bool IsEarning { get; set; } // Earning or Deduction
    public bool IsStatic { get; set; } // System-defined (cannot delete) or Custom
    public bool IsActive { get; set; } = true;
}