using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class AttendanceLog
{
    public long LogId { get; set; }

    public int EmployeeId { get; set; }

    public int OrganizationId { get; set; }

    public DateTime LogTimestamp { get; set; }

    public int LogType { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? IpAddress { get; set; }

    public string? DeviceInfo { get; set; }

    public bool IsProcessed { get; set; }

    public DateTime CreatedAt { get; set; }
}
