using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class RolePermission
{
    public int RolePermissionId { get; set; }

    public int OrgRoleId { get; set; }

    public int PermissionId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrgRole OrgRole { get; set; } = null!;

    public virtual PermissionMaster Permission { get; set; } = null!;
}
