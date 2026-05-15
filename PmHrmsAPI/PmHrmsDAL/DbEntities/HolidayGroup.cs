namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public class HolidayGroup
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string GroupName { get; set; } = null!; 
    public int Year { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<HolidayGroupMapping> GroupHolidays { get; set; } = new List<HolidayGroupMapping>();
    public virtual ICollection<HolidayGroupEligibility> EligibilityRules { get; set; } = new List<HolidayGroupEligibility>();
}
