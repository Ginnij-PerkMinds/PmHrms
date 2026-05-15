namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class OrganizationResponseModel
    {
        public int OrgId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string OfficialEmail { get; set; } = string.Empty;
        public string ContactPhoneNo { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public string? LogoUrl { get; set; }

        public string ? RegistrationNumber { get; set; }
        public string ? TaxId { get; set; }

        public string? FaviconUrl { get; set; }
        public string? AddressLine1 { get; set; } 
        public string? AddressLine2 { get; set; }
        public string? ZipCode { get; set; }


        public string? City { get; set; }
        public string? StateName { get; set; }
        public string? CountryName { get; set; }
        public string? FullAddress { get; set; }
    }
}