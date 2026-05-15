using System;
using System.Collections.Generic;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public int OrganizationId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? CheckInIp { get; set; }

    public string? CheckOutIp { get; set; }

    public decimal? CheckInLatitude { get; set; }

    public decimal? CheckInLongitude { get; set; }

    public decimal? CheckOutLatitude { get; set; }

    public decimal? CheckOutLongitude { get; set; }

    public AttendanceStatus Status { get; set; }

    public int TotalWorkingMinutes { get; set; }

    public bool IsManualEntry { get; set; }

    public DateTime CreatedAt { get; set; }


    public bool IsVoided { get; set; }

    public string? VoidReason { get; set; }

    public DateTime? VoidedAt { get; set; }
}
