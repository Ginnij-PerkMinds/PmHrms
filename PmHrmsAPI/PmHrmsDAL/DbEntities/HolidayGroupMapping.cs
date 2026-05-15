namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class HolidayGroupMapping
{
    public int Id { get; set; }
    public int HolidayGroupId { get; set; }
    public int SystemHolidayId { get; set; }
    
    
    public bool IsOptional { get; set; } 

    // Navigation Properties
    public virtual HolidayGroup HolidayGroup { get; set; } = null!;
    public virtual SystemHoliday SystemHoliday { get; set; } = null!;
}