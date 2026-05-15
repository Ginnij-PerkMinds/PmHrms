namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class OrganizationHolidayConfigResponseModel
    {
        public int Year { get; set; }
        public List<int> AvailableYears { get; set; } = new();
        public List<HolidayOfficeLocationResponseModel> OfficeLocations { get; set; } = new();
        public List<OrganizationHolidayItemResponseModel> Holidays { get; set; } = new();
    }                                                                                               

    public class HolidayOfficeLocationResponseModel
    {
        public int OfficeLocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class OrganizationHolidayItemResponseModel
    {
        public int SystemHolidayId { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public DateOnly HolidayDate { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsCustom { get; set; }
        public bool IsDefaultActive { get; set; }
        public List<int> OfficeLocationIds { get; set; } = new();
    }
}
