namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class SystemField
    {
        public string Key { get; set; } // e.g., "first_name"
        public string Label { get; set; } // e.g., "First Name"
        public string Description { get; set; }
        public bool? Required { get; set; }
        public string Category { get; set; }

        
        public string? ValidationType { get; set; }
        public string Keywords { get; set; }
    }
}
