using Microsoft.AspNetCore.Identity.Data;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models.Auth;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterCompany(RegisterCompanyModel request);
        Task<string?> Login(LoginRequest request , bool isOtpLogin);
        Task<string?> VerifyOtp(VerifyOtpModel request);
        Task CreateEmployeeLogin(int employeeId, string defaultPassword,
                                                int? systemRoleId, int? orgRoleId);
        Task<string?> ResendOtp(string email);
        Task<string> SendSignupOtp(string email);
        Task<bool> VerifySignupOtp(string email, string otp);
        Task<string?> CreateGhostOrg(string organizationName, string ipAddress);
        Task<string?> CheckGhostByDevice(string ipAddress);

        Task<string?> RenewGhostToken(int employeeId);

    }
}