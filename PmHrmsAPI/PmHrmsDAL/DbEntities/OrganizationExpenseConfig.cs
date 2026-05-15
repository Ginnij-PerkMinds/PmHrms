using System;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OrganizationExpenseConfig
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ExpenseTypeId { get; set; }
    public decimal? MaxLimit { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ExpenseMaster ExpenseType { get; set; } = null!;
}