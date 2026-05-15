using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class RoleLayoutAccess
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int LayoutId { get; set; }

    public bool IsAllowed { get; set; }

    public int? SystemRoleId { get; set; }

    public virtual Layout Layout { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual SystemRole? SystemRole { get; set; }
}
