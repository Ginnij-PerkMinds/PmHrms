using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class OrganizationModel
    {
        [Required(ErrorMessage = "Organization Name is required")]
        public string OrganizationName { get; set; } = string.Empty;

        public string? TagLine { get; set; }

        [Required]
        [EmailAddress]
        public string OfficialEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string ContactPhoneNo { get; set; } = string.Empty;

        public string? WebsiteUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? FaviconUrl { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? TaxId { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }

       
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public string? ZipCode { get; set; }
    }
}