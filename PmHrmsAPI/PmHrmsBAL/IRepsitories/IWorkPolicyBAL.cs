using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IWorkPolicyBAL
    {
        Task<PagedResult<WorkPolicyResponseModel>> GetAll(
         int page,
         int size,
         string? search);
        Task<WorkPolicyResponseModel?> GetById(int id);
        Task<WorkPolicyResponseModel> Create(WorkPolicyModel model);
        Task<WorkPolicyResponseModel?> Update(int id, WorkPolicyModel model);
        Task<bool> Delete(int id);

        Task RemovePolicyFromDesignation(int designationId, int policyId);

        Task AssignPolicyToDesignation(int designationId, int policyId);
        Task<WorkPolicyResult?> GetWorkPolicyByEmployeeId(int employeeId);

        Task<List<DesignationPolicyMappingResponse>> GetDesignationPolicyMappings();
    }
}
