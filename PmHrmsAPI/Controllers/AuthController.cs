using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Models.Auth;
using System.Runtime.CompilerServices;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService   , ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("RegisterCompany")] 
        public async Task<IActionResult> Register([FromBody] RegisterCompanyModel request)
        {
            try { return Ok(new ApiResponseModel<string>(true, await _authService.RegisterCompany(request), null)); }
            catch (Exception ex) { return StatusCode(500, new ApiResponseModel<string>(false, "Registration Failed", ex.Message)); }
        }

        [HttpPost("Login")] 
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            bool isOtpLogin = string.IsNullOrEmpty(request.Password) || request.Password == "OTP_LOGIN";

            var otpResult = await _authService.Login(request, isOtpLogin);

            if (otpResult == null)
                return Unauthorized(new ApiResponseModel<string>(false, "Invalid Credentials", null));

            return Ok(new ApiResponseModel<string>(true, "OTP Sent", otpResult));
        }

        [HttpPost("VerifyOtp")] 
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel request)
        {
            var token = await _authService.VerifyOtp(request);
            if (token == null) return Unauthorized(new ApiResponseModel<string>(false, "Invalid OTP", null));
            return Ok(new ApiResponseModel<string>(true, "Login Successful", token));
        }




        [HttpPost("ResendOtp")]
        public async Task<IActionResult> ResendOtp([FromBody] VerifyOtpModel request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new ApiResponseModel<string>(false, "Email is required", null));

            var message = await _authService.ResendOtp(request.Email);

            if (message == null)
                return NotFound(new ApiResponseModel<string>(false, "User not found or no pending verification", null));

            return Ok(new ApiResponseModel<string>(true, message, null)); 
        }


        [HttpPost("Signup/SendOtp")]
        public async Task<IActionResult> SendSignupOtp([FromBody] EmailRequestModel request)
        {
            try
            {
                var otp = await _authService.SendSignupOtp(request.Email);
                
                return Ok(new ApiResponseModel<string>(
                    true,
                    "OTP Sent to Email",
                    null));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseModel<string>(false, ex.Message, null));
            }
        }

        
        [HttpPost("Signup/VerifyOtp")]
        public async Task<IActionResult> VerifySignupOtp([FromBody] VerifyOtpModel request)
        {
            var isVerified = await _authService.VerifySignupOtp(request.Email, request.Otp);
            if (!isVerified)
                return BadRequest(new ApiResponseModel<string>(false, "Invalid or Expired OTP", null));

            return Ok(new ApiResponseModel<string>(true, "Email Verified. Please proceed to fill company details.", null));
        }


        [HttpGet("CheckGhostByIP")]
        public async Task<IActionResult> CheckGhost()
        {
            string deviceId = Request.Headers["x-device-id"].FirstOrDefault() ?? string.Empty;
            var token = await _authService.CheckGhostByDevice(deviceId);


            return Ok(new ApiResponseModel<object>(true, "Success", new
            {
                exists = token != null,
                token = token
            }));
        }


        [HttpGet("RenewGhostToken/{empId}")]
        public async Task<IActionResult> RenewGhost(int empId)
        {
            try
            {
                var newToken = await _authService.RenewGhostToken(empId);

                if (newToken == null)
                    return Unauthorized(new ApiResponseModel<string>(false, "Renewal not allowed for this account", null));

                return Ok(new ApiResponseModel<string>(true, "Token Renewed Successfully", newToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Renewal Failed", ex.Message));
            }
        }

        [HttpPost("CreateGhostOrg")]
        public async Task<IActionResult> CreateGhost([FromBody] GhostOrgRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OrganizationName))
            {
                return BadRequest(new ApiResponseModel<string>(false, "Company name is required.", string.Empty));
            }

            string orgName = request.OrganizationName.Trim();
            string deviceId = Request.Headers["x-device-id"].FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return BadRequest(new ApiResponseModel<string>(false, "Device id is required.", string.Empty));
            }
             

            var token = await _authService.CreateGhostOrg(orgName, deviceId);
            if (string.IsNullOrWhiteSpace(token))
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Account creation failed.", string.Empty));
            }

            return Ok(new ApiResponseModel<string>(true, "Account Created", token));
        }

         
    }
}
 
