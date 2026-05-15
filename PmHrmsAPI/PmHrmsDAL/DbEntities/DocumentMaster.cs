using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class DocumentMaster
{
    public int DocumentId { get; set; }

    public string DocumentKey { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public bool? IsExpiryRequired { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<OrganizationDocumentRequirement> OrganizationDocumentRequirements { get; set; } = new List<OrganizationDocumentRequirement>();
}
