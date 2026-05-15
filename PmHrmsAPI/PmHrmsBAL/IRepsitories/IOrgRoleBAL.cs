using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    
    public interface IOrgRoleBAL
    {
        Task<RoleBundleResponse> GetRoles(int orgId);
        Task<ApiResponseModel<OrgRole>> CreateRole(string name, int orgId);
        Task<ApiResponseModel<OrgRole>> UpdateRole(int id, string name);
        Task<bool> DeleteRole(int id);
    }
}
