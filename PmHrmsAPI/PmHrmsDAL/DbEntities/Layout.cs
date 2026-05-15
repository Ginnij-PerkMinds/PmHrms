using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Layout
{
    public int LayoutId { get; set; }

    public string LayoutKey { get; set; } = null!;

    public string LayoutName { get; set; } = null!;

    public virtual ICollection<RoleLayoutAccess> RoleLayoutAccesses { get; set; } = new List<RoleLayoutAccess>();
}
