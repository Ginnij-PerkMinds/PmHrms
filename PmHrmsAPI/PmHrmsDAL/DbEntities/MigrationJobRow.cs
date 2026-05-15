using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class MigrationJobRow
{
    public int Id { get; set; }

    public Guid JobId { get; set; }

    public string RowHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MigrationJob Job { get; set; } = null!;
}
