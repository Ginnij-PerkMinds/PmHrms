using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OfficeLocation
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public string? AllowedIpAddress { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? GeoRadiusMeters { get; set; }

    public int? OrganizationId { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Organization? Organization { get; set; }
}
