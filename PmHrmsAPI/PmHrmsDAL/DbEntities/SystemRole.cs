using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class SystemRole
{
    public int SystemRoleId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    public virtual ICollection<RoleLayoutAccess> RoleLayoutAccesses { get; set; } = new List<RoleLayoutAccess>();
}
