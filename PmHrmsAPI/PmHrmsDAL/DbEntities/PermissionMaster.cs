using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class PermissionMaster
{
    public int PermissionId { get; set; }

    public string ModuleName { get; set; } = null!;

    public string PermissionKey { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsSystemDefault { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
