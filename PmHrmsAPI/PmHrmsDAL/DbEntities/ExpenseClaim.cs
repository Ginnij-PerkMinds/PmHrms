using System;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class ExpenseClaim
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganizationId { get; set; }
    public int ExpenseTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string FilePath { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? Remarks { get; set; }

    public virtual Employee User { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
    public virtual ExpenseMaster ExpenseType { get; set; } = null!;
}