using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class ShiftMaster
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? GracePeriodMinutes { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
