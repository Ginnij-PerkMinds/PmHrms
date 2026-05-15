using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class EmployeePolicyMapping
{
    public int MappingId { get; set; }

    public int EmployeeId { get; set; }

    public int PolicyId { get; set; }

    public int OrganizationId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
