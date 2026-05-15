using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IRoleLayoutAccessBAL
    {
        Task<DefaultLayoutResponse> GetDefaultLayoutForRole(int roleId);
        Task<List<RoleLayoutAccessResponse>> GetRoleLayoutAccess(int roleId);
        Task<RoleLayoutAccessResponse> AssignLayout(RoleLayoutAccessRequest request);
    }
}
