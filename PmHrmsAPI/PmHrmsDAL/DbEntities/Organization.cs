using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Organization
{
    public int OrgId { get; set; }

    public string OrganizationName { get; set; } = null!;

    public string? TagLine { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? OfficialEmail { get; set; }

    public string? ContactPhoneNo { get; set; }

    public string? LogoUrl { get; set; }

    public string? FaviconUrl { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? TaxId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public int? StateId { get; set; }

    public int? CountryId { get; set; }

    public string? ZipCode { get; set; }

    public bool IsSetupCompleted { get; set; }

    public bool IsActive { get; set; }

    public bool IdGhost { get; set; }

    public string? CreatedByIp { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<OfficeLocation> OfficeLocations { get; set; } = new List<OfficeLocation>();

    public virtual ICollection<OrgRole> OrgRoles { get; set; } = new List<OrgRole>();

    public virtual ICollection<OrganizationDocumentRequirement> OrganizationDocumentRequirements { get; set; } = new List<OrganizationDocumentRequirement>();

    public virtual State? State { get; set; }
}
