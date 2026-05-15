using Microsoft.AspNetCore.Http;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IOrganizationBAL
    {
        Task<(List<OrganizationResponseModel>, int totalCount)> GetAllOrganization(int pageNumber, int pageSize, string? searchTerm);
        Task<OrganizationResponseModel?> GetOrganization(int id);
        Task<OrganizationResponseModel?> AddOrganization(OrganizationModel request);
        Task<OrganizationResponseModel?> UpdateOrganization(int id, OrganizationModel request);
        Task<OrgSetupStatusResponse?> GetOrgSetupStatus(int orgId);
        Task<DashboardStatsResponse> GetDashboardStats(int orgId);
        Task<string?> UploadOrganizationLogo(int orgId, IFormFile file);
        Task<bool> DeleteOrganizationLogo(int orgId);
        Task<bool> DeleteOrganization(int id);
    }
}
