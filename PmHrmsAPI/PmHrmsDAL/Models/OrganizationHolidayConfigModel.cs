namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class SaveOrganizationHolidayConfigModel
    {
        public int Year { get; set; }
        public List<SaveOrganizationHolidayItemModel> Holidays { get; set; } = new();
    }

    public class SaveOrganizationHolidayItemModel
    {
        public int SystemHolidayId { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public DateOnly? HolidayDate { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsCustom { get; set; }
        public bool IsDefaultActive { get; set; }
        public List<int> OfficeLocationIds { get; set; } = new();
    }


        public class ToggleHolidayRequest
        {
            public int SystemHolidayId { get; set; }
            public int? OfficeLocationId { get; set; } // null = company default
            public bool IsActive { get; set; }
        }

       



}
