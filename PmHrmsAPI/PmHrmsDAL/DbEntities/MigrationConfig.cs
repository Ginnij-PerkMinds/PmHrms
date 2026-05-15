using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class MigrationConfig
{
    public int Id { get; set; }

    public string EntityType { get; set; } = null!;

    public string FieldKey { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? ValidationType { get; set; }

    public string? Keywords { get; set; }

    public bool? IsRequired { get; set; }

    public string? Category { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? OrgId { get; set; }
}
