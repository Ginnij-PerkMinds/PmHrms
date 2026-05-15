namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class EmployeeDetailResponseModel
    {
        public int DetailId { get; set; }
        public int EmployeeId { get; set; }

        public DateOnly? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }
        public string? FatherName { get; set; }

        public string? PanNumber { get; set; }
        public string? AadharNumber { get; set; }
        public string? PassportNumber { get; set; }

        public string? CurrentAddressLine { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentZipCode { get; set; }

        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }

        
        public int? CurrentStateId { get; set; }
        public string? CurrentStateName { get; set; }

        public int? CurrentCountryId { get; set; }
        public string? CurrentCountryName { get; set; }

       
        public object? OfficeLocation { get; set; }
    }
}