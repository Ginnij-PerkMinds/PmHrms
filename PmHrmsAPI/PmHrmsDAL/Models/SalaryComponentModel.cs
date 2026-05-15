public class SalaryComponentModel
{
    public int ComponentMasterId { get; set; }
    public string ComponentName { get; set; } = null!;
 
    public decimal Amount { get; set; }                            

    public bool IsEarning { get; set; } // true = earning, false = deduction
}       