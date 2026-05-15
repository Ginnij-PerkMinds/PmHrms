using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OrgRole
{
    public int OrgRoleId { get; set; }

    public int? OrgId { get; set; }

    public string? Name { get; set; }

    public virtual Organization? Org { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
