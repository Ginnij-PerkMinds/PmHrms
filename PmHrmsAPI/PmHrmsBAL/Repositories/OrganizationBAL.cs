using Microsoft.AspNetCore.Http;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using PmHrmsAPI.PmHrmsFAL.IRespositories;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class OrganizationBAL : IOrganizationBAL
    {
        //private const string CustomHolidayCountryCode = "CSTM";
        private const string CustomHolidayCountryCode = PmHrmsConstants.OrganizationMessages.CustomHolidayCountryCode;

        private readonly OrganizationDAL _organizationDAL;
        private readonly IPermissionService _permissionService;
        private readonly IImageFAL _imageFAL;

        public OrganizationBAL(
            OrganizationDAL organizationDAL,
            IPermissionService permissionService,
            IImageFAL imageFAL)
        {
            _organizationDAL = organizationDAL;
            _permissionService = permissionService;
            _imageFAL = imageFAL;
        }

        public async Task<(List<OrganizationResponseModel>, int totalCount)> GetAllOrganization(int pageNumber, int pageSize, string? searchTerm)
        {
            _permissionService.Ensure(PermissionKeys.ORG_VIEW);
            var (entities, count) = await _organizationDAL.GetAllOrganization(pageNumber, pageSize, searchTerm);

            var responseList = entities.Select(o => new OrganizationResponseModel
            {
                OrgId = o.OrgId,
                OrganizationName = o.OrganizationName,
                OfficialEmail = o.OfficialEmail ?? "",
                ContactPhoneNo = o.ContactPhoneNo ?? "",
                WebsiteUrl = o.WebsiteUrl,
                LogoUrl = o.LogoUrl,
                City = o.City,
                StateName = o.State?.StateName,
                CountryName = o.Country?.CountryName,
                FullAddress = $"{o.AddressLine1}, {o.City}"
            }).ToList();

            return (responseList, count);
        }

        public async Task<OrganizationResponseModel?> GetOrganization(int id)
        {
          //  _permissionService.Ensure(PermissionKeys.ORG_VIEW);
            var entity = await _organizationDAL.GetOrganization(id);
            if (entity == null) return null;

            return new OrganizationResponseModel
            {
                OrgId = entity.OrgId,
                OrganizationName = entity.OrganizationName,
                OfficialEmail = entity.OfficialEmail ?? "",
                ContactPhoneNo = entity.ContactPhoneNo ?? "",
                WebsiteUrl = entity.WebsiteUrl,
                LogoUrl = entity.LogoUrl,
                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                ZipCode = entity.ZipCode,
                City = entity.City,
                StateName = entity.State?.StateName,
                CountryName = entity.Country?.CountryName,
                FullAddress = $"{entity.AddressLine1} {entity.AddressLine2}, {entity.City}".Trim()
            };
        }

        public async Task<OrganizationResponseModel?> UpdateOrganization(int id, OrganizationModel request)
        {
            _permissionService.Ensure(PermissionKeys.ORG_EDIT);

            var entityToUpdate = new Organization
            {
                OrgId = id,
                OrganizationName = request.OrganizationName,
                TagLine = request.TagLine,
                OfficialEmail = request.OfficialEmail,
                ContactPhoneNo = request.ContactPhoneNo,
                WebsiteUrl = request.WebsiteUrl,
                LogoUrl = request.LogoUrl,
                FaviconUrl = request.FaviconUrl,
                RegistrationNumber = request.RegistrationNumber,
                TaxId = request.TaxId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                StateId = request.StateId,
                CountryId = request.CountryId,
                ZipCode = request.ZipCode
            };

            var result = await _organizationDAL.UpdateOrganization(entityToUpdate);
            if (result == null) return null;

            return await GetOrganization(result.OrgId);
        }

        public async Task<OrganizationResponseModel?> AddOrganization(OrganizationModel request)
        {
            _permissionService.Ensure(PermissionKeys.ORG_EDIT);

            var newEntity = new Organization
            {
                OrganizationName = request.OrganizationName,
                TagLine = request.TagLine,
                OfficialEmail = request.OfficialEmail,
                ContactPhoneNo = request.ContactPhoneNo,
                WebsiteUrl = request.WebsiteUrl,
                LogoUrl = request.LogoUrl,
                FaviconUrl = request.FaviconUrl,
                RegistrationNumber = request.RegistrationNumber,
                TaxId = request.TaxId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                StateId = request.StateId,
                CountryId = request.CountryId,
                ZipCode = request.ZipCode
            };

            var result = await _organizationDAL.AddOrganization(newEntity);
            return await GetOrganization(result.OrgId);
        }

        public async Task<OrgSetupStatusResponse?> GetOrgSetupStatus(int orgId)
        {
            _permissionService.Ensure(PermissionKeys.ORG_SETUP_WIZARD);
            var admin = await _organizationDAL.GetAdminEmployee(orgId);
            if (admin == null) return null;

            var user = await _organizationDAL.GetUserByEmployeeId(admin.EmployeeId);

            //bool email = !admin.OfficialEmail.Contains("@perkminds.temp");
            bool email = !admin.OfficialEmail.Contains(PmHrmsConstants.OrganizationMessages.TempEmailDomain);

            //bool first = admin.FirstName != "Guest";
            bool first = admin.FirstName != PmHrmsConstants.OrganizationMessages.GuestFirstName;

            //bool last = !string.IsNullOrWhiteSpace(admin.LastName) && admin.LastName != "Admin";
            bool last = !string.IsNullOrWhiteSpace(admin.LastName) && admin.LastName != PmHrmsConstants.OrganizationMessages.AdminLastName;

            //bool pass = user != null && !user.PasswordHash.StartsWith("GHOST_LOCKED");
            bool pass = user != null && !user.PasswordHash.StartsWith(PmHrmsConstants.OrganizationMessages.GhostLockedPassword);

            bool policy = admin.PolicyId != null;          
            bool office = admin.AssignedOfficeId != null;

            SetupStep step;

            if (!email) step = SetupStep.EMAIL;
            else if (!first) step = SetupStep.FIRST_NAME;
            else if (!last) step = SetupStep.LAST_NAME;
            else if (!pass) step = SetupStep.PASSWORD;
            else if (!policy) step = SetupStep.WORK_POLICY;      
            else if (!office) step = SetupStep.OFFICE_LOCATION;
            else step = SetupStep.COMPLETED;

            if (step == SetupStep.COMPLETED)
                await _organizationDAL.MarkOrgSetupCompleted(orgId);

            return new OrgSetupStatusResponse 
            { 
                IsSetupCompleted = step == SetupStep.COMPLETED,
                NextStep = step,
               ProgressPercentage = CalculateProgress(email, first, last, pass, policy, office) // ✅ updated
    }; 
        }

       
        public async Task<DashboardStatsResponse> GetDashboardStats(int orgId)
        {
            // Ensure they have permission to view the dashboard
            _permissionService.Ensure(PermissionKeys.DASHBOARD_ADMIN_VIEW);

            var stats = await _organizationDAL.GetDashboardStatsAsync(orgId);

            return new DashboardStatsResponse
            {
                TotalEmployees = stats.empCount,
                TotalDepartments = stats.deptCount,
                TotalDesignations = stats.desigCount,
                RecentDepartments = stats.recentDepts
            };
}

        public async Task<string?> UploadOrganizationLogo(int orgId, IFormFile file)
        {
            _permissionService.Ensure(PermissionKeys.ORG_EDIT);

            if (file == null || file.Length == 0)
            {
                //throw new ArgumentException("Logo file is required");
                throw new ArgumentException(PmHrmsConstants.OrganizationMessages.LogoFileRequired);
            }

            var existingOrg = await _organizationDAL.GetOrganization(orgId);
            if (existingOrg == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(existingOrg.LogoUrl))
            {
                _imageFAL.DeleteImage(existingOrg.LogoUrl);
            }

            var uploadedPath = await UploadOrgLogoWithFallback(file);
            var isUpdated = await _organizationDAL.UpdateOrganizationLogo(orgId, uploadedPath);

            return isUpdated ? uploadedPath : null;
        }

        public async Task<bool> DeleteOrganizationLogo(int orgId)
        {
            _permissionService.Ensure(PermissionKeys.ORG_EDIT);

            var existingOrg = await _organizationDAL.GetOrganization(orgId);
            if (existingOrg == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(existingOrg.LogoUrl))
            {
                _imageFAL.DeleteImage(existingOrg.LogoUrl);
            }

            return await _organizationDAL.UpdateOrganizationLogo(orgId, null);
        }

      

        public async Task<bool> DeleteOrganization(int id)
        {
            _permissionService.Ensure(PermissionKeys.ORG_DELETE);
            var result = await _organizationDAL.DeleteOrganization(id);
            return result;
        }

        private int CalculateProgress(bool email, bool first, bool last, bool pass, bool policy, bool office)
            {
                int score = 10;
                if (email)  score += 15;
                if (first)  score += 15;
                if (last)   score += 15;
                if (pass)   score += 15;
                if (policy) score += 15;  
                if (office) score += 15;  
                return score;
            }

        private async Task<string> UploadOrgLogoWithFallback(IFormFile file)
        {
            try
            {
                //return await _imageFAL.UploadImageAsync(file, "CompanyLogosDir");
                return await _imageFAL.UploadImageAsync(file, PmHrmsConstants.FolderNames.CompanyLogos);
            }
            catch
            {
                //return await _imageFAL.UploadImageAsync(file, "ProfilePicsDir");
                return await _imageFAL.UploadImageAsync(file, PmHrmsConstants.FolderNames.ProfilePics);
            }
        }
    }
}
