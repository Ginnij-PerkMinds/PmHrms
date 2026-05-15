
﻿using Microsoft.Extensions.Logging;
﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using PmHrmsAPI.PmHrmsFAL.IRespositories;
using PmHrmsAPI.PmHrmsFAL.Repositories;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;



namespace PmHrmsAPI.PmHrmsBAL
{
    public class EmployeeBAL : IEmployeeBAL
    {
        private readonly EmployeeDAL _employeeDAL;
        private readonly OrganizationDAL _organizationDAL;
        private readonly IAuthService _authService;
        private readonly IImageFAL _imageFAL;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<EmployeeBAL> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmployeeOnboardingService _onboarding;
        private readonly OfficeLocationDAL _officeLocationDAL;
        private readonly IWorkPolicyBAL _workPolicyBAL;



        public EmployeeBAL(
      EmployeeDAL employeeDAL,
      IAuthService authService,
      IImageFAL imageFAL,
      OrganizationDAL organizationDAL,
      ILogger<EmployeeBAL> logger,
      IPermissionService permissionService,
      IHttpContextAccessor httpContextAccessor,
      IEmployeeOnboardingService onboarding,
      OfficeLocationDAL officeLocationDAL , 
      IWorkPolicyBAL workPolicyBAL)  
        {
            _employeeDAL = employeeDAL;
            _authService = authService;
            _imageFAL = imageFAL;
            _organizationDAL = organizationDAL;
            _permissionService = permissionService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _onboarding = onboarding;
            _workPolicyBAL = workPolicyBAL;
            _officeLocationDAL = officeLocationDAL; 
        }


        public async Task<PagedResult<EmployeeResponseModel>> GetAllEmployees(int page, int size, string? search)
        {
            _permissionService.Ensure(PermissionKeys.EMP_VIEW);
            var (entities, count) = await _employeeDAL.GetAllEmployees(page, size, search);

            var items = entities.Select(e =>
            {
                var currentUser = e.AppUsers
                    .OrderByDescending(u => u.UserId)
                    .FirstOrDefault();

                return new EmployeeResponseModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = $"{e.FirstName} {e.LastName}".Trim(),
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    OrganizationId = e.OrganizationId,
                    PhoneNumber = e.PhoneNumber,
                    AltPhoneNumber = e.AltPhoneNumber,
                    OfficialEmail = e.OfficialEmail,
                    DateOfJoining = e.DateOfJoining,
                    EmploymentStatus = e.EmploymentStatus,
                    WorkMode = e.WorkMode,
                    NoticePeriodDays = e.NoticePeriodDays,
                    IsActive = e.IsActive,
                    OfficialImageUrl = e.OfficialImageUrl,
                    DepartmentId = e.DepartmentId,
                    DesignationId = e.DesignationId,
                    AssignedOfficeId = e.AssignedOfficeId,
                    ShiftId = e.ShiftId,
                    ReportingManagerId = e.ReportingManagerId,
                    SecondaryManagerId = e.SecondaryManagerId,
                    DepartmentName = e.Department?.DepartmentName,
                    DesignationName = e.Designation?.DesignationName,
                    ReportingManagerName = e.ReportingManager != null
                        ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}"
                        : null,
                    SecondaryManagerName = e.SecondaryManager != null
                        ? $"{e.SecondaryManager.FirstName} {e.SecondaryManager.LastName}".Trim()
                        : null,
                    SystemRoleId = currentUser?.SystemRoleId,
                    SystemRoleName = currentUser?.SystemRole?.Name,
                    OrgRoleId = currentUser?.OrgRoleId,
                    OrgRoleName = currentUser?.OrgRole?.Name,
                    PolicyId = e.PolicyId,
                    PolicyName = e.Policy?.PolicyName,
                };
            }).ToList();

            return new PagedResult<EmployeeResponseModel>
            {
                Items = items,
                TotalCount = count,
                PageNumber = page,
                PageSize = size
            };
  }
        public async Task<List<EmployeeResponseModel>> GetTeamMembers(int employeeId)
        {
            var emp = await _employeeDAL.GetEmployee(employeeId);

            if (emp == null || emp.DepartmentId == null)
                return new List<EmployeeResponseModel>();


            var members = await _employeeDAL.GetTeamMembers(
                emp.DepartmentId.Value,
                employeeId
            );

            return members.Select(e => new EmployeeResponseModel
            {
                EmployeeId = e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}".Trim(),
                FirstName = e.FirstName,
                LastName = e.LastName,
                OfficialEmail = e.OfficialEmail,
                PhoneNumber = e.PhoneNumber,
                OfficialImageUrl = e.OfficialImageUrl,
                DesignationName = e.Designation?.DesignationName
            }).ToList();
        }

        public async Task<bool> PhoneExists(string phone)
        {
            return await _employeeDAL.PhoneExists(phone);
        }
        public async Task<EmployeeResponseModel?> GetEmployee(int id)
        {
            _logger.LogInformation("GetEmployee called for id: {Id}.", id);
            try
            {
                 if (!_permissionService.IsSelf(id))
                    {
                        _permissionService.Ensure(PermissionKeys.EMP_PROFILE_VIEW);
                    }
               
                var e = await _employeeDAL.GetEmployee(id);
                if (e == null)
                {
                    _logger.LogWarning("Employee not found for id: {Id}.", id);
                    return null;
                }
                var currentUser = e.AppUsers
                    .OrderByDescending(u => u.UserId)
                    .FirstOrDefault();


                 var policyResult = await _workPolicyBAL.GetWorkPolicyByEmployeeId(e.EmployeeId);
                 var locationResult = await GetEmployeeOfficeLocation(e.EmployeeId); 

                var response = new EmployeeResponseModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = $"{e.FirstName} {e.LastName}".Trim(),
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    OfficialEmail = e.OfficialEmail,
                    PhoneNumber = e.PhoneNumber,
                    AltPhoneNumber = e.AltPhoneNumber,
                    DateOfJoining = e.DateOfJoining,
                    OrganizationId = e.OrganizationId,
                    EmploymentStatus = e.EmploymentStatus,
                    WorkMode = e.WorkMode,
                    NoticePeriodDays = e.NoticePeriodDays,
                    IsActive = e.IsActive,
                    OfficialImageUrl = e.OfficialImageUrl,
                    DepartmentId = e.DepartmentId,
                    DesignationId = e.DesignationId,
                    ReportingManagerId = e.ReportingManagerId,
                    SecondaryManagerId = e.SecondaryManagerId,
                    AssignedOfficeId = e.AssignedOfficeId,
                    ShiftId = e.ShiftId,
                    DepartmentName = e.Department?.DepartmentName,
                    DesignationName = e.Designation?.DesignationName,
                    ReportingManagerName = e.ReportingManager != null
    ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}"
    : null,
                    ReportingManagerImageUrl = e.ReportingManager?.OfficialImageUrl,

                    ReportingManagerEmail = e.ReportingManager?.OfficialEmail,
                    ReportingManagerPhone = e.ReportingManager?.PhoneNumber,
                    SecondaryManagerName = e.SecondaryManager != null ? $"{e.SecondaryManager.FirstName} {e.SecondaryManager.LastName}" : null,
                    SystemRoleId = currentUser?.SystemRoleId,
                    SystemRoleName = currentUser?.SystemRole?.Name,
                    OrgRoleId = currentUser?.OrgRoleId,
                    OrgRoleName = currentUser?.OrgRole?.Name,
                    WorkPolicy = policyResult == null ? null : new WorkPolicyInfo
                    {
                        PolicyId = policyResult.Policy.PolicyId,
                        PolicyName = policyResult.Policy.PolicyName,
                        Source = policyResult.Source
                    },
                    OfficeLocation = locationResult == null ? null : new OfficeLocationInfo
                    {
                        LocationId = locationResult.Location.LocationId,
                        LocationName = locationResult.Location.LocationName,
                        Source = locationResult.Source
                    },

                    PersonalDetails = e.EmployeeDetail != null ? new EmployeeDetailResponseModel
                    {
                        DetailId = e.EmployeeDetail.DetailId,
                        EmployeeId = e.EmployeeDetail.EmployeeId,
                        DateOfBirth = e.EmployeeDetail.DateOfBirth,
                        BloodGroup = e.EmployeeDetail.BloodGroup,
                        MaritalStatus = e.EmployeeDetail.MaritalStatus,
                        FatherName = e.EmployeeDetail.FatherName,
                        PanNumber = e.EmployeeDetail.PanNumber,
                        AadharNumber = e.EmployeeDetail.AadharNumber,
                        PassportNumber = e.EmployeeDetail.PassportNumber,
                        CurrentAddressLine = e.EmployeeDetail.CurrentAddressLine,
                        CurrentCity = e.EmployeeDetail.CurrentCity,
                        CurrentZipCode = e.EmployeeDetail.CurrentZipCode,
                        LinkedinUrl = e.EmployeeDetail.LinkedinUrl,
                        GithubUrl = e.EmployeeDetail.GithubUrl,
                        CurrentStateId = e.EmployeeDetail.CurrentStateId,
                        CurrentStateName = e.EmployeeDetail.CurrentState?.StateName,
                        CurrentCountryId = e.EmployeeDetail.CurrentCountryId,
                        CurrentCountryName = e.EmployeeDetail.CurrentCountry?.CountryName
                    } : null
                };
                _logger.LogInformation("Retrieved employee for id: {Id}.", id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetEmployee for id: {Id}.", id);
                throw;
            }
        }
        public async Task<OfficeLocationResult?> GetEmployeeOfficeLocation(int employeeId)
        {
            var employee = await _employeeDAL.GetEmployee(employeeId);

            if (employee == null)
                return null;

           if (employee.AssignedOfficeId.HasValue)
            {
                var loc = await _employeeDAL.GetOfficeLocation(employee.AssignedOfficeId.Value);

                if (loc != null)
                {
                    return new OfficeLocationResult
                    {
                        Location = loc,
                        Source = PmHrmsConstants.OfficeLocationSource.EMPLOYEE_LEVEL
                    };
                }
            }

            //  Default fallback
             var defaultLoc = await _officeLocationDAL.GetDefaultLocationAsync(employee.OrganizationId);

            if (defaultLoc != null)
            {
                return new OfficeLocationResult
                {
                    Location = defaultLoc,
                    Source = PmHrmsConstants.OfficeLocationSource.DEFAULT
                };
            }
            return null ; 
        }
        public async Task<EmployeeResponseModel?> AddEmployee(EmployeeModel request)
        {
            _permissionService.Ensure(PermissionKeys.EMP_CREATE);

         
            if (string.IsNullOrWhiteSpace(request.EmployeeCode))
                //throw new ArgumentException("Employee code is required");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.EmployeeCodeRequired);

            if (string.IsNullOrWhiteSpace(request.FirstName))
                //throw new ArgumentException("First name is required");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.FirstNameRequired);

            if (string.IsNullOrWhiteSpace(request.OfficialEmail))
                //throw new ArgumentException("Official email is required");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.OfficialEmailRequired);

           
            request.EmployeeCode = request.EmployeeCode.Trim();
            request.FirstName = request.FirstName.Trim();
            request.LastName = request.LastName?.Trim();
            request.OfficialEmail = request.OfficialEmail.Trim();
            request.PhoneNumber = request.PhoneNumber?.Trim();

          
            var orgIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("OrgId")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim))
                //throw new Exception("Organization ID not found in token");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.OrgIdNotFound);

            var orgId = int.Parse(orgIdClaim);

            
            if (await _employeeDAL.EmployeeCodeExists(request.EmployeeCode, orgId))
                //throw new ArgumentException("Employee code already exists");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.EmployeeCodeExists);

            if (await _employeeDAL.EmailExists(request.OfficialEmail))
                //throw new ArgumentException("Official email already exists");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.OfficialEmailExists);

            
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (await _employeeDAL.PhoneExists(request.PhoneNumber))
                    //throw new ArgumentException("Phone number already exists");
                    throw new ArgumentException(PmHrmsConstants.EmployeeMessages.PhoneNumberExists);
            }

           
            if (!string.IsNullOrEmpty(request.PhoneNumber) &&
                !Regex.IsMatch(request.PhoneNumber, @"^\+?[0-9]{10,15}$"))
                //throw new ArgumentException("Invalid phone number format");
                throw new ArgumentException(PmHrmsConstants.EmployeeMessages.InvalidPhoneFormat);

            string? imageUrl = null;

            if (request.ProfileImage != null)
                imageUrl = await _imageFAL.UploadImageAsync(request.ProfileImage, "ProfilePicsDir");

         
            var entity = new Employee
            {
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                OfficialEmail = request.OfficialEmail,
                PhoneNumber = request.PhoneNumber,
                AltPhoneNumber = request.AltPhoneNumber,
                DateOfJoining = request.DateOfJoining,
                OrganizationId = orgId,
                EmploymentStatus = request.EmploymentStatus,
                WorkMode = request.WorkMode,
                NoticePeriodDays = request.NoticePeriodDays,
                OfficialImageUrl = imageUrl,
                DepartmentId = request.DepartmentId,
                DesignationId = request.DesignationId,
                AssignedOfficeId = request.AssignedOfficeId == 0 ? null : request.AssignedOfficeId,
                ShiftId = request.ShiftId == 0 ? null : request.ShiftId,
                ReportingManagerId = request.ReportingManagerId == 0 ? null : request.ReportingManagerId,
                SecondaryManagerId = request.SecondaryManagerId == 0 ? null : request.SecondaryManagerId,
                PolicyId = request.PolicyId == 0 ? null : request.PolicyId,
                IsActive = true
            };

            try
            {
                var result = await _employeeDAL.AddEmployee(entity);

                await _onboarding.ApplyDefaultsAsync(result, orgId);
                await _employeeDAL.UpdateEmployee(result);

                _logger.LogInformation(
                    "[AddEmployee] Defaults applied | EmpId {EmpId} | Office {Office} | Policy {Policy}",
                    result.EmployeeId, result.AssignedOfficeId, result.PolicyId);

              
                if (request.SystemRoleId > 0 && request.OrgRoleId > 0)
                {
                    try
                    {
                        var randomPassword = GenerateRandomPassword();

                        await _authService.CreateEmployeeLogin(
                            result.EmployeeId,
                            randomPassword,
                            request.SystemRoleId,
                            request.OrgRoleId
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[AddEmployee] Login creation failed for EmpId {Id}", result.EmployeeId);
                    }
                }

                return await GetEmployee(result.EmployeeId);
            }
            catch (DbUpdateException dbEx)
            {
                var error = dbEx.InnerException?.Message.ToLower();

                if (error != null)
                {
                    if (error.Contains("employee_code"))
                        //throw new ArgumentException("Employee code already exists");
                        throw new ArgumentException(PmHrmsConstants.EmployeeMessages.EmployeeCodeExists);

                    if (error.Contains("official_email"))
                        //throw new ArgumentException("Official email already exists");
                        throw new ArgumentException(PmHrmsConstants.EmployeeMessages.OfficialEmailExists);

                    if (error.Contains("phone"))
                        //throw new ArgumentException("Phone number already exists");
                        throw new ArgumentException(PmHrmsConstants.EmployeeMessages.PhoneNumberExists);
                }

                throw;
            }
        }



        public async Task<EmployeeResponseModel?> UpdateEmployee(int id, EmployeeModel request)
        {

            _permissionService.EnsureCanActOn(PermissionKeys.EMP_EDIT_OFFICIAL, id);

            var existingEmployee = await _employeeDAL.GetEmployee(id);
            if (existingEmployee == null) return null;


            existingEmployee.FirstName = request.FirstName.Trim();
            existingEmployee.LastName = request.LastName?.Trim();
            existingEmployee.OfficialEmail = request.OfficialEmail;
            existingEmployee.DepartmentId = request.DepartmentId;
            existingEmployee.DesignationId = request.DesignationId;

            existingEmployee.ReportingManagerId = (request.ReportingManagerId == 0) ? null : request.ReportingManagerId;
            existingEmployee.SecondaryManagerId = (request.SecondaryManagerId == 0) ? null : request.SecondaryManagerId;
            existingEmployee.ShiftId = (request.ShiftId == 0) ? null : request.ShiftId;
            existingEmployee.AssignedOfficeId = (request.AssignedOfficeId == 0) ? null : request.AssignedOfficeId;

            existingEmployee.EmploymentStatus = request.EmploymentStatus;
            existingEmployee.WorkMode = request.WorkMode;
            existingEmployee.NoticePeriodDays = request.NoticePeriodDays;
            existingEmployee.PolicyId = request.PolicyId;

            if (request.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(existingEmployee.OfficialImageUrl))
                {
                    _imageFAL.DeleteImage(existingEmployee.OfficialImageUrl);
                }
                existingEmployee.OfficialImageUrl = await _imageFAL.UploadImageAsync(request.ProfileImage, "ProfilePicsDir");
            }

            if (request.OrgRoleId > 0)
            {
                var selectedRole = await _employeeDAL.GetOrgRoleById(request.OrgRoleId);
                if (selectedRole == null || selectedRole.OrgId != existingEmployee.OrganizationId)
                {
                    //throw new ArgumentException("Selected organization role is invalid for this employee");
                    throw new ArgumentException(PmHrmsConstants.EmployeeMessages.InvalidOrgRole);
                }

                var employeeUser = await _organizationDAL.GetUserByEmployeeId(existingEmployee.EmployeeId);
                if (employeeUser == null)
                {
                    //throw new ArgumentException("Employee login account not found for role assignment");
                    throw new ArgumentException(PmHrmsConstants.EmployeeMessages.EmployeeLoginNotFound);
                }

                if (employeeUser.OrgRoleId != request.OrgRoleId)
                {
                    _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
                    employeeUser.OrgRoleId = request.OrgRoleId;
                    await _organizationDAL.UpdateUser(employeeUser);
                }
            }

            var result = await _employeeDAL.UpdateEmployee(existingEmployee);
            if (result == null) return null;

            return await GetEmployee(result.EmployeeId);
       }





        public async Task<bool> DeleteEmployee(int id)
        {
            _logger.LogInformation("DeleteEmployee called for id: {Id}.", id);
            try
            {
                _permissionService.Ensure(PermissionKeys.EMP_DELETE);
                var success = await _employeeDAL.DeleteEmployee(id);
                if (success)
                {
                    _logger.LogInformation("Employee deleted successfully for id: {Id}.", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete employee for id: {Id}.", id);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteEmployee for id: {Id}.", id);
                throw;
            }
        }

        public async Task<EmployeeResponseModel?> UpdateAdminProfile(AdminProfileUpdateModel request)
        {
            _logger.LogInformation("UpdateAdminProfile called for organizationId: {OrgId}.", request.OrganizationId);
            try
            {
                _permissionService.Ensure(PermissionKeys.ORG_EDIT);
                var existing = await _organizationDAL.GetAdminEmployee(request.OrganizationId);
                if (existing == null)
                {
                    _logger.LogWarning("Admin employee not found for organizationId: {OrgId}.", request.OrganizationId);
                    return null;
                }

                var isUpdatingOfficialInfo =
                    !string.IsNullOrWhiteSpace(request.FirstName) ||
                    !string.IsNullOrWhiteSpace(request.LastName) ||
                    !string.IsNullOrWhiteSpace(request.OfficialEmail);

                if (isUpdatingOfficialInfo)
                {
                    var isSetupCompleted = await _organizationDAL.IsOrgSetupCompleted(request.OrganizationId);
                    if (isSetupCompleted)
                    {
                        _permissionService.EnsureNotSelf(existing.EmployeeId, "update official details");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.FirstName))
                    existing.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.LastName))
                    existing.LastName = request.LastName;
                if (!string.IsNullOrWhiteSpace(request.OfficialEmail))
                    existing.OfficialEmail = request.OfficialEmail;

                 if (request.PolicyId.HasValue && request.PolicyId.Value > 0)
                         existing.PolicyId = request.PolicyId.Value;

                if (request.AssignedOfficeId.HasValue && request.AssignedOfficeId.Value > 0)
                      existing.AssignedOfficeId = request.AssignedOfficeId.Value;

                var result = await _employeeDAL.UpdateEmployee(existing);
                if (!string.IsNullOrEmpty(request.Password))
                { 
                    var user = await _organizationDAL.GetUserByEmployeeId(existing.EmployeeId);
                    if (user != null)                               
                    {                                                   
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                        user.IsFirstLogin = false;
                        await _organizationDAL.UpdateUser(user);
                        _logger.LogInformation("Updated password for admin employee id: {EmployeeId}.", existing.EmployeeId);
                    }                   
                }

                _logger.LogInformation("Admin profile updated successfully for employee id: {EmployeeId}.", result.EmployeeId);
                return await GetEmployee(result.EmployeeId);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateAdminProfile for organizationId: {OrgId}.", request.OrganizationId);
                throw;
            }
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _employeeDAL.EmailExists(email);
        }

        public async Task<bool> EmployeeCodeExists(string code, int orgId)
        {
            return await _employeeDAL.EmployeeCodeExists(code, orgId);
        }

        public async Task<RoleBundleResponse> GetRoles(int orgId)
        {
            _logger.LogInformation("GetRoles called for orgId: {OrgId}.", orgId);
            try
            {
                _permissionService.Ensure(PermissionKeys.ROLE_MANAGE);
                var systemRoles = await _employeeDAL.GetSystemRoles();
                var orgRoles = await _employeeDAL.GetOrgRoles(orgId);
                _logger.LogInformation("Retrieved {SystemRolesCount} system roles and {OrgRolesCount} org roles for orgId: {OrgId}.", systemRoles.Count, orgRoles.Count, orgId);
                return new RoleBundleResponse
                {
                    SystemRoles = systemRoles.Select(r => new RoleResponseModel
                    {
                        RoleId = r.SystemRoleId,
                        RoleName = r.Name
                    }).ToList(),
                    OrgRoles = orgRoles.Select(r => new RoleResponseModel
                    {
                        RoleId = r.OrgRoleId,
                        RoleName = r.Name
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetRoles for orgId: {OrgId}.", orgId);
                throw;
            }
        }

        public async Task<string?> UpdateEmployeeProfileImage(int id, IFormFile file)
        {
            _logger.LogInformation("UpdateEmployeeProfileImage called for id: {Id}.", id);
            try
            {
                 if (!_permissionService.IsSelf(id))
                    {
                        _permissionService.Ensure(PermissionKeys.EMP_EDIT_PERSONAL);
                    }
                var emp = await _employeeDAL.GetEmployee(id);
                if (emp == null)
                {
                    _logger.LogWarning("Employee not found for profile image update, id: {Id}.", id);
                    return null;
                }
                if (!string.IsNullOrEmpty(emp.OfficialImageUrl))
                {
                    _imageFAL.DeleteImage(emp.OfficialImageUrl);
                    _logger.LogInformation("Deleted old profile image for employee id: {Id}.", id);
                }
                var newUrl = await _imageFAL.UploadImageAsync(file, "ProfilePicsDir");
                emp.OfficialImageUrl = newUrl;
                await _employeeDAL.UpdateEmployee(emp);
                _logger.LogInformation("Profile image updated successfully for employee id: {Id}.", id);
                return newUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateEmployeeProfileImage for id: {Id}.", id);
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeProfileImage(int id)
        {
            _logger.LogInformation("DeleteEmployeeProfileImage called for id: {Id}.", id);
            try
            {
               if (!_permissionService.IsSelf(id))
                {
                    _permissionService.Ensure(PermissionKeys.EMP_EDIT_PERSONAL);
                }
                var emp = await _employeeDAL.GetEmployee(id);
                if (emp == null)
                {
                    _logger.LogWarning("Employee not found for profile image deletion, id: {Id}.", id);
                    return false;
                }
                if (!string.IsNullOrEmpty(emp.OfficialImageUrl))
                {
                    _imageFAL.DeleteImage(emp.OfficialImageUrl);
                    _logger.LogInformation("Deleted profile image for employee id: {Id}.", id);
                }
                emp.OfficialImageUrl = null;
                await _employeeDAL.UpdateEmployee(emp);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteEmployeeProfileImage for id: {Id}.", id);
                throw;
            }
        }

        private string GenerateRandomPassword()
        {
            int length = 8;
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();


            while (length-- > 0)

                res.Append(validChars[rnd.Next(validChars.Length)]);

            return res.ToString();
        }

    }
}
