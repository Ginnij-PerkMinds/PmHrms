using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public int? HeadOfDepartmentId { get; set; }

    public int OrganizationId { get; set; }

    public bool IsSystemDefault { get; set; }

    public bool IsActive { get; set; }

    public string? DepartmentNameNormalized { get; set; }

    public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual Employee? HeadOfDepartment { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
