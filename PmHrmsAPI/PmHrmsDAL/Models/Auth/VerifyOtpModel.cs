namespace PmHrmsAPI.PmHrmsDAL.Models.Auth
{
    public class VerifyOtpModel
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}