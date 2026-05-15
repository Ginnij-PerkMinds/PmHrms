using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class MigrationJob
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? TotalRecords { get; set; }

    public int? ValidatedCount { get; set; }

    public int? ImportedCount { get; set; }

    public int? FailedCount { get; set; }

    public string? CurrentStep { get; set; }

    public string? ErrorLog { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? RequestedByUserId { get; set; }

    public int OrgId { get; set; }

    public string? ValidationSummary { get; set; }

    public string? FileName { get; set; }

    public DateTime? LastHeartbeat { get; set; }

    public string? SharedPassword { get; set; }

    public string? MappingJson { get; set; }

    public virtual ICollection<MigrationJobRow> MigrationJobRows { get; set; } = new List<MigrationJobRow>();
}
