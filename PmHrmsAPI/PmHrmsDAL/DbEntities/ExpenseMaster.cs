using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class ExpenseMaster
{
    public int Id { get; set; }
    public string TypeName { get; set; } = null!;
    public decimal DefaultMaxLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}