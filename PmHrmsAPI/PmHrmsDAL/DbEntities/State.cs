using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class State
{
    public int StateId { get; set; }

    public string StateName { get; set; } = null!;

    public int? CountryId { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<EmployeeDetail> EmployeeDetails { get; set; } = new List<EmployeeDetail>();

    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
