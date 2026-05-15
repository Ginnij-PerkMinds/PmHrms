using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class EmployeeRemoteLocation
{
    public int RemoteId { get; set; }

    public int EmployeeId { get; set; }

    public string? LocationAlias { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int? GeoRadiusMeters { get; set; }

    public bool? IsActive { get; set; }

    public int? ApprovedById { get; set; }

    public virtual Employee? ApprovedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
