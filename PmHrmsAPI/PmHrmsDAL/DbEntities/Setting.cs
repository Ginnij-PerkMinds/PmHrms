using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Setting
{
    public int SettingId { get; set; }

    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;

    public bool? Active { get; set; }

    public DateTime? CreatedAt { get; set; }
}
