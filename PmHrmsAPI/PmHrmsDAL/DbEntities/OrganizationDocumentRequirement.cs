using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OrganizationDocumentRequirement
{
    public int RequirementId { get; set; }

    public int OrganizationId { get; set; }

    public string DocumentType { get; set; } = null!;

    public bool IsMandatory { get; set; }

    public DateTime CreatedAt { get; set; }

    public int DocumentMasterId { get; set; }

    public virtual DocumentMaster DocumentMaster { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
