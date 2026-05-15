using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OrgWorkSchedule
{
    public int ScheduleId { get; set; }

    public int OrganizationId { get; set; }

    public int WorkingDaysMask { get; set; }

    public DateTime CreatedAt { get; set; }
}
