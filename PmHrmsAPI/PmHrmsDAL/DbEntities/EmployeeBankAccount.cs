using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class EmployeeBankAccount
{
    public int BankAccountId { get; set; }

    public int EmployeeId { get; set; }   
    public int OrganizationId { get; set; } 

    public string AccountHolderName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string IFSCCode { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string? BranchName { get; set; }

    public bool IsPrimary { get; set; } = true; 

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Employee Employee { get; set; } = null!;
}