namespace PmHrmsAPI.PmHrmsDAL.Models
{
    
    public class SaveHolidayGroupRequest
    {
        public int GroupId { get; set; } 
        public string GroupName { get; set; } = null!; 
        public int Year { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        
        public List<int> SystemHolidayIds { get; set; } = new();

       
        public List<GroupEligibilityRule> EligibilityRules { get; set; } = new();
    }

    public class GroupEligibilityRule
    {
        public int? OfficeLocationId { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class AddCustomHolidayRequest
    {
        public string HolidayName { get; set; } = null!;
        public DateOnly HolidayDate { get; set; }
        public int Year { get; set; }
        public bool IsRecurring { get; set; }
    }
}
