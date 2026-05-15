using PmHrmsAPI.PmHrmsDAL.DbEntities;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.Models
{

public class SalaryResult
{
    public SalaryStructure Salary { get; set; } = null!;
    public SalarySource Source { get; set; }

    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    public SalaryResult(SalaryStructure salary, SalarySource source)
    {
        Salary = salary;
        Source = source;

        TotalEarnings = salary.Components
            .Where(x => x.IsEarning)
            .Sum(x => x.Amount);

        TotalDeductions = salary.Components
            .Where(x => !x.IsEarning)
            .Sum(x => x.Amount);

        NetSalary = TotalEarnings - TotalDeductions;
    }
}
}