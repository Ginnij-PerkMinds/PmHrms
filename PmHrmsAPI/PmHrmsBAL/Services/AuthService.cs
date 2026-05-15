using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces; 
using PmHrmsAPI.PmHrmsDAL;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangfire;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    
    public class AuthService : IAuthService
    {
        private readonly PmHrmsContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthService(PmHrmsContext context,
         IConfiguration config ,
          IEmailService emailService ,
          ILogger<AuthService> logger,
           IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _logger = logger;
             _backgroundJobClient = backgroundJobClient;
        }

        public async Task<string> RegisterCompany(RegisterCompanyModel request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var email = request.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required.");

            _logger.LogInformation(
                "RegisterCompany started. Email: {Email}, Company: {CompanyName}",
                email,
                request.CompanyName
            );

            var verification = await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.Target == email &&
                    x.VerificationType == "EMAIL_SIGNUP"
                );

            if (verification == null || verification.IsVerified != true)
                throw new Exception("Email not verified. Please verify OTP first.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var org = new Organization
                {
                    OrganizationName = request.CompanyName,
                    OfficialEmail = email,
                    ContactPhoneNo = request.Phone,
                    IsActive = true,
                    IdGhost = false,
                    CreatedByIp = null
                };
                _context.Organizations.Add(org);
                await _context.SaveChangesAsync();

                var ownerRole = new OrgRole { OrgId = org.OrgId, Name = "Owner" };
                var hrRole = new OrgRole { OrgId = org.OrgId, Name = "HR" };
                var employeeRole = new OrgRole { OrgId = org.OrgId, Name = "Employee" };
                _context.OrgRoles.AddRange(ownerRole, hrRole, employeeRole);
                await _context.SaveChangesAsync();

                var dept = new Department
                {
                    DepartmentName = "Administration",
                    OrganizationId = org.OrgId,
                    IsSystemDefault = true
                };
                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();

                var desig = new Designation
                {
                    DesignationName = "Admin",
                    DepartmentId = dept.DepartmentId,
                    HierarchyLevel = 1,
                    IsSystemDefault = true
                };
                _context.Designations.Add(desig);
                await _context.SaveChangesAsync();

                var emp = new Employee
                {
                    OrganizationId = org.OrgId,
                    FirstName = request.AdminName,
                    OfficialEmail = email,
                    EmployeeCode = "ADMIN01",
                    DateOfJoining = DateOnly.FromDateTime(DateTime.Now),
                    DepartmentId = dept.DepartmentId,
                    DesignationId = desig.DesignationId,
                    IsActive = true
                };
                _context.Employees.Add(emp);
                await _context.SaveChangesAsync();

                var legacyRoleId = await _context.Roles
                    .Where(r => r.RoleName == "SuperAdmin")
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (legacyRoleId == 0)
                {
                    legacyRoleId = await _context.Roles
                        .OrderBy(r => r.RoleId)
                        .Select(r => r.RoleId)
                        .FirstOrDefaultAsync();
                }

                var systemRoleId = await _context.SystemRoles
                    .Where(r => r.Name == "SuperAdmin")
                    .Select(r => (int?)r.SystemRoleId)
                    .FirstOrDefaultAsync();

                if (!systemRoleId.HasValue)
                {
                    systemRoleId = await _context.SystemRoles
                        .OrderBy(r => r.SystemRoleId)
                        .Select(r => (int?)r.SystemRoleId)
                        .FirstOrDefaultAsync();
                }

                var user = new AppUser
                {
                    EmployeeId = emp.EmployeeId,
                    RoleId = legacyRoleId == 0 ? 2 : legacyRoleId,
                    SystemRoleId = systemRoleId,
                    OrgRoleId = ownerRole.OrgRoleId,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    IsFirstLogin = false
                };
                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();

                await AssignDefaultPermissions(ownerRole.OrgRoleId);

                _context.OtpVerifications.Remove(verification);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation(
                    "RegisterCompany completed successfully. OrgId: {OrgId}, EmployeeId: {EmployeeId}, UserId: {UserId}",
                    org.OrgId,
                    emp.EmployeeId,
                    user.UserId
                );

                return "Company Registered Successfully";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "RegisterCompany failed for Email: {Email}", email);
                throw;
            }
        }

        
        public async Task<string?> Login(LoginRequest request , bool isOtpLogin)
        {
            _logger.LogInformation("Login attempt started for Email: {Email}", request.Email);

            var emp = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.OfficialEmail == request.Email);
            if (emp == null)
            {
                _logger.LogWarning("Login failed: User with Email {Email} not found.", request.Email);
                return null;
            }

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.EmployeeId == emp.EmployeeId);
            if (user == null) return null;


            if (!isOtpLogin)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return null; 
                }
            }


            // Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.Now.AddMinutes(5);
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP generated successfully for {Email}", request.Email);

            _backgroundJobClient.Enqueue<IEmailService>(service =>
            service.SendEmailAsync(new MailRequest
            {
                ToEmail = emp.OfficialEmail,
                Subject = "PmHrms Login OTP",
                Body = $"<h3>Your Login OTP is: {otp}</h3><p>Valid for 5 minutes.</p>"
            }));

           return otp;
        }



        public async Task<string> SendSignupOtp(string email)
        {
            email = email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required.");

            if (await _context.Employees
                .IgnoreQueryFilters()
                .AnyAsync(e => e.OfficialEmail == email))
                throw new Exception("Email already registered.");

            string otp = new Random().Next(100000, 999999).ToString();

            var existingEntry = await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.Target == email &&
                    x.VerificationType == "EMAIL_SIGNUP"
                );


            if (existingEntry != null)
            {
                existingEntry.OtpCode = otp;
                existingEntry.OtpExpiry = DateTime.Now.AddMinutes(10);
                existingEntry.IsVerified = false; 
                existingEntry.VerificationType = "EMAIL_SIGNUP"; 
            }
            else
            {
                _context.OtpVerifications.Add(new OtpVerification
                {
                    Target = email, 
                    OtpCode = otp,
                    OtpExpiry = DateTime.Now.AddMinutes(10),
                    IsVerified = false,
                    VerificationType = "EMAIL_SIGNUP"
                });
            }

            await _context.SaveChangesAsync();


           _backgroundJobClient.Enqueue<IEmailService>(service =>
    service.SendEmailAsync(new MailRequest
    {
        ToEmail = email,
        Subject = "PmHrms Registration OTP",
        Body = $"<h3>Your Registration OTP is: {otp}</h3>"
    }));

           

            return otp;
        }



        public async Task<bool> VerifySignupOtp(string email, string otp)
        {
            email = email?.Trim() ?? string.Empty;

            var entry = await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.Target == email &&
                    x.VerificationType == "EMAIL_SIGNUP"
                );


            if (entry == null || entry.OtpCode != otp || entry.OtpExpiry < DateTime.Now)
                return false;

            entry.IsVerified = true;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<string?> VerifyOtp(VerifyOtpModel request)
        {
            var emp = await _context.Employees
                 .IgnoreQueryFilters()
                 .FirstOrDefaultAsync(e => e.OfficialEmail == request.Email);
            if (emp == null) return null;

            var user = await _context.AppUsers
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                .Include(u => u.Employee)
            .ThenInclude(e => e.Organization)
            .FirstOrDefaultAsync(u => u.EmployeeId == emp.EmployeeId);

            if (user == null)
            {
                _logger.LogWarning("User record not found for EmployeeId: {Id}", emp.EmployeeId);
                return null;
            }

            if (user.OtpCode != request.Otp)
            {
                _logger.LogWarning("OTP Mismatch. DB: {Stored}, Input: {Input}", user.OtpCode, request.Otp);
                return null;
            }

            if (user.OtpExpiry < DateTime.Now)
            {
                _logger.LogWarning("OTP Expired for {Email}", request.Email);
                return null;
            }

            user.OtpCode = null;
            await _context.SaveChangesAsync();

            return GenerateJwtToken(user);
        }


        public async Task CreateEmployeeLogin(  int employeeId, string defaultPassword,
                                                int? systemRoleId,  int? orgRoleId)
          {

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return; 

                int finalRoleId;


            var user = new AppUser
            {
                EmployeeId = employeeId,
                SystemRoleId = systemRoleId ?? 2, 
                OrgRoleId = orgRoleId,
                RoleId = 2,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword)
            };

                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();


                _backgroundJobClient.Enqueue<IEmailService>(service =>
    service.SendEmailAsync(new MailRequest
    {
        ToEmail = employee.OfficialEmail,
        Subject = "PmHrms Account Credentials",
        Body = $@"<h3>Welcome to PmHrms!</h3>
                 <p>Your account has been created successfully.</p>
                 <p><strong>Login ID:</strong> {employee.OfficialEmail}</p>
                 <p><strong>Temporary Password:</strong> {defaultPassword}</p>
                 <hr>
                 <p>Please login and change your password immediately.</p>"

            }));
       
        }



        public async Task<string?> ResendOtp(string email)
        {            
            if (string.IsNullOrWhiteSpace(email))          
                return null;             

            email = email.Trim();     
            _logger.LogInformation("ResendOtp requested for Email: {Email}", email);     

            var signupEntry = await _context.OtpVerifications                                      
                .FirstOrDefaultAsync(x =>
                    x.Target == email &&
                    x.VerificationType == "EMAIL_SIGNUP" &&
                    x.IsVerified == false
                );

            if (signupEntry != null)
            {
                string signupOtp = new Random().Next(100000, 999999).ToString();
                signupEntry.OtpCode = signupOtp;
                signupEntry.OtpExpiry = DateTime.Now.AddMinutes(10);
                await _context.SaveChangesAsync();

                _backgroundJobClient.Enqueue<IEmailService>(service =>
                    service.SendEmailAsync(new MailRequest
                    {
                        ToEmail = email,
                        Subject = "Resend: PmHrms Registration OTP",
                        Body = $"<h3>Your New Signup OTP is: {signupOtp}</h3>"
                    }));

                _logger.LogInformation("ResendOtp completed for signup flow. Email: {Email}", email);
                return "OTP_SENT";
            }

            var emp = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.OfficialEmail == email);

            if (emp == null)
            {
                _logger.LogWarning("ResendOtp failed. Employee not found for Email: {Email}", email);
                return null;
            }

            var user = await _context.AppUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.EmployeeId == emp.EmployeeId);

            if (user == null)
            {
                _logger.LogWarning("ResendOtp failed. AppUser not found for EmployeeId: {EmployeeId}", emp.EmployeeId);
                return null;
            }

            string loginOtp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = loginOtp;
            user.OtpExpiry = DateTime.Now.AddMinutes(5);
            await _context.SaveChangesAsync();

           _backgroundJobClient.Enqueue<IEmailService>(service =>
            service.SendEmailAsync(new MailRequest
            {
                ToEmail = email,
                Subject = "Resend: PmHrms Login OTP",
                Body = $"<h3>Your New Login OTP is: {loginOtp}</h3>"
            }));

            _logger.LogInformation("ResendOtp completed for login flow. Email: {Email}", email);
            return "OTP_SENT";
        }


       

        public async Task<string?> CheckGhostByDevice(string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID))
            {
                return null;
            }
           
            var org = await _context.Organizations
                .FirstOrDefaultAsync(o => o.CreatedByIp == deviceID && o.IdGhost == true);

            if (org == null) return null;

            
            var user = await _context.AppUsers
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                .Include(u => u.SystemRole)
                .Include(u => u.OrgRole)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Organization)
                .FirstOrDefaultAsync(u => u.Employee.OrganizationId == org.OrgId);

            return user != null ? GenerateJwtToken(user) : null;
        }

        public async Task<string?> CreateGhostOrg(string organizationName, string deviceID)
        {
            organizationName = organizationName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException("Company name is required.");
            }

            if (string.IsNullOrWhiteSpace(deviceID))
            {
                throw new ArgumentException("Device id is required.");
            }

            var existingToken = await CheckGhostByDevice(deviceID);
            if (!string.IsNullOrWhiteSpace(existingToken))
            {
                return existingToken;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {


                // 1. New Organization
                var org = new Organization
                {
                    OrganizationName = organizationName,
                    IdGhost = true,
                    CreatedByIp = deviceID,
                    IsActive = true,
                    OfficialEmail = $"temp_{Guid.NewGuid().ToString().Substring(0, 6)}@perkminds.temp"
                };

                _context.Organizations.Add(org);
                await _context.SaveChangesAsync();
                
                var ownerRole = new OrgRole { OrgId = org.OrgId, Name = "Owner" };
                var hrRole = new OrgRole { OrgId = org.OrgId, Name = "HR" };
                var employeeRole = new OrgRole { OrgId = org.OrgId, Name = "Employee" };

               _context.OrgRoles.AddRange(ownerRole, hrRole, employeeRole);
                await _context.SaveChangesAsync();

               


                var dept = new Department
                {
                    DepartmentName = "Administration",
                    OrganizationId = org.OrgId,
                    IsSystemDefault = true,
                    IsActive = true,
                    Organization = org
                };


                // Default Designation
                var desig = new Designation
                {
                    DesignationName = "Admin",
                    HierarchyLevel = 1,
                    IsSystemDefault = true,
                    IsActive = true,
                    Department = dept
                };

               
                // 2. Dummy Employee
                var emp = new Employee
                {
                    Organization = org,
                    Department = dept,
                    Designation = desig,
                    PhoneNumber = "0000000000",
                    FirstName = "Guest",
                    LastName = "Admin",
                    OfficialEmail = org.OfficialEmail,
                    EmployeeCode = "EMP-" + Guid.NewGuid().ToString()[..8].ToUpper(),  
                    IsActive = true,
                    DateOfJoining = DateOnly.FromDateTime(DateTime.Now)
                   
                };


                // 3. Ghost AppUser (Locked Password)
                var adminRole = await _context.Roles
                    .Where(r => r.RoleName == "SuperAdmin")
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                var systemRoleId = await _context.SystemRoles
                   .Where(r => r.Name == "SuperAdmin")
                    .Select(r => (int?)r.SystemRoleId)
                    .FirstOrDefaultAsync();

                if (!systemRoleId.HasValue)
                {
                    systemRoleId = await _context.SystemRoles
                        .OrderBy(r => r.SystemRoleId)
                        .Select(r => (int?)r.SystemRoleId)
                        .FirstOrDefaultAsync();
                }


                var user = new AppUser
                {
                    Employee = emp,

                    RoleId = adminRole == 0 ? 2 : adminRole, // legacy

                    SystemRoleId = systemRoleId,   
                    OrgRoleId = ownerRole.OrgRoleId, 

                    PasswordHash = "GHOST_LOCKED_" + Guid.NewGuid()
                };


                _context.AddRange( dept, desig, emp, user);


                await _context.SaveChangesAsync();
                await AssignDefaultPermissions(ownerRole.OrgRoleId);

                await transaction.CommitAsync();


                // Reload to get navigation properties for JWT claims
                var userWithData = await _context.AppUsers
                  .IgnoreQueryFilters()
                  .Include(u => u.Role)
                  .Include(u => u.SystemRole)
                  .Include(u => u.OrgRole)
                  .Include(u => u.Employee)
                      .ThenInclude(e => e.Organization)
                  .FirstOrDefaultAsync(u => u.UserId == user.UserId);

               

                if (userWithData == null)
                {
                    throw new InvalidOperationException("Ghost user was created but could not be loaded.");
                }

                return GenerateJwtToken(userWithData);
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                _logger.LogError(ex, "Ghost Org Creation failed");
                throw;
            }
        }


        
        public async Task<string?> RenewGhostToken(int employeeId)
        {
            var user = await _context.AppUsers
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.Organization)
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);

          
            if (user != null && user.Employee.Organization.IdGhost == true)
            {
                _logger.LogInformation("Renewing JWT for Ghost User ID: {Id}", employeeId);
                return GenerateJwtToken(user);
            }

            return null;
        }


        private async Task AssignDefaultPermissions(int orgRoleId)
        {
            var defaultPermissions = await _context.PermissionMasters
                .Where(p => p.IsSystemDefault == true)
                .Select(p => p.PermissionId)
                .ToListAsync();

            var rolePermissions = defaultPermissions.Select(permissionId =>
                new RolePermission
                {
                    OrgRoleId = orgRoleId, 
                    PermissionId = permissionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

            _context.RolePermissions.AddRange(rolePermissions);
            await _context.SaveChangesAsync();
        }



        private string GenerateJwtToken(AppUser user)
        {
            if (user == null)
                throw new Exception("User is null while generating JWT");

            if (user.Employee == null)
                throw new Exception("Employee navigation not loaded");

            if (user.Employee.Organization == null)
                throw new Exception("Organization navigation not loaded");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Employee.OfficialEmail),
        new Claim("EmployeeId", user.EmployeeId.ToString()),
        new Claim("Role", user.Role?.RoleName ?? "Employee"),
        new Claim("OrgId", user.Employee.OrganizationId.ToString()),
        new Claim("RoleId", user.RoleId.ToString()),
        new Claim("SystemRoleId", user.SystemRoleId?.ToString() ?? "0"),
        new Claim("OrgRoleId", user.OrgRoleId?.ToString() ?? "0"),
        new Claim("SystemRole", user.SystemRole?.Name ?? ""),
        new Claim("OrgRole", user.OrgRole?.Name ?? ""),



        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: user.Employee.Organization.IdGhost ? DateTime.Now.AddDays(10) : DateTime.Now.AddDays(3),
        signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
