using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class AppUser
{
    public int UserId { get; set; }

    public int EmployeeId { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? OtpCode { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public bool? IsFirstLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RoleId { get; set; }

    public int? SystemRoleId { get; set; }

    public int? OrgRoleId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual OrgRole? OrgRole { get; set; } = null!;

    public virtual SystemRole? SystemRole { get; set; }
}
