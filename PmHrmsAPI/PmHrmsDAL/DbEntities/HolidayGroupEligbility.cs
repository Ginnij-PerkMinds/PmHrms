namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class HolidayGroupEligibility
{
    public int Id { get; set; }
    public int HolidayGroupId { get; set; }
    
    
    public int? OfficeLocationId { get; set; } 
    public int? DepartmentId { get; set; } 

    // Navigation Properties
    public virtual HolidayGroup HolidayGroup { get; set; } = null!;    
    public virtual OfficeLocation? OfficeLocation { get; set; }
    public virtual Department? Department { get; set; } 
}