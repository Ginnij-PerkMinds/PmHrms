namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class SystemHoliday
{
    public int Id { get; set; }
    public DateOnly HolidayDate { get; set; }
    public string HolidayName { get; set; } = null!;
    public int Year { get; set; }
    public string CountryCode { get; set; } = "IN"; 
    
    public bool IsRecurring { get; set; }
    
    
    public bool IsCustom { get; set; } 
    public int? CreatedByOrgId { get; set; } 

    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public virtual Organization? CreatedByOrg { get; set; }
     public ICollection<HolidayGroupMapping> HolidayMappings { get; set; }

}