using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string OfficialEmail { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? AltPhoneNumber { get; set; }

    public DateOnly DateOfJoining { get; set; }

    public string? EmploymentStatus { get; set; }

    [Column("work_mode")]
    [Obsolete("Use PolicyId and the WorkPolicy table for new features.")]

    public string? WorkMode { get; set; }

    public DateOnly? ResignationDate { get; set; }

    public DateOnly? LastWorkingDay { get; set; }

    public int? NoticePeriodDays { get; set; }

    public string? ExitReason { get; set; }

    public int? AssignedOfficeId { get; set; }

    public string? OfficialImageUrl { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ShiftId { get; set; }

    public int? ReportingManagerId { get; set; }

    public int? SecondaryManagerId { get; set; }

    public bool IsActive { get; set; }

    public int OrganizationId { get; set; }

    public int? PolicyId { get; set; }

    public int? SalaryStructureId { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    public virtual OfficeLocation? AssignedOffice { get; set; }

    public virtual Department? Department { get; set; }
    public virtual Designation? Designation { get; set; }
    public virtual SalaryStructure? SalaryStructure {get; set;}
    

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual EmployeeDetail? EmployeeDetail { get; set; }

    public virtual ICollection<EmployeeDocument> EmployeeDocumentEmployees { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<EmployeeDocument> EmployeeDocumentVerifiedBies { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<EmployeeRemoteLocation> EmployeeRemoteLocationApprovedBies { get; set; } = new List<EmployeeRemoteLocation>();

    public virtual ICollection<EmployeeRemoteLocation> EmployeeRemoteLocationEmployees { get; set; } = new List<EmployeeRemoteLocation>();

    public virtual ICollection<Employee> InverseReportingManager { get; set; } = new List<Employee>();

    public virtual ICollection<Employee> InverseSecondaryManager { get; set; } = new List<Employee>();

    public ICollection<Post> CreatedPosts { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual WorkPolicy? Policy { get; set; }

    public virtual Employee? ReportingManager { get; set; }

    public virtual Employee? SecondaryManager { get; set; }

    public virtual ShiftMaster? Shift { get; set; }

    public int? HolidayGroupId { get; set; } 
    public virtual HolidayGroup? HolidayGroup { get; set; }
}
