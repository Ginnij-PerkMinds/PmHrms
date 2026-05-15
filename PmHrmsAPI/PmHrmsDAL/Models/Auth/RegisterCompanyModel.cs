namespace PmHrmsAPI.PmHrmsDAL.Models.Auth
{
    public class RegisterCompanyModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}