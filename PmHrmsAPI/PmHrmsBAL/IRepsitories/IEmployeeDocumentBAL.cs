using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IEmployeeDocumentBAL
    {
        Task<PagedResult<EmployeeDocumentResponseModel>> GetAllDocuments(int page, int size, string? search, int? employeeId, int orgId);
        Task<List<EmployeeDocumentResponseModel>> GetDocumentsByEmployeeId(int employeeId);
        Task<bool> AddDocument(EmployeeDocumentModel request);
        Task<bool> VerifyDocument(DocumentVerificationModel request);
        Task<bool> ReuploadDocument(int docId, EmployeeDocumentModel request, IFormFile file);
        Task<List<OrganizationDocumentRequirementResponseModel>> GetRequirementsByOrgId(int orgId);
        Task<bool> AddDocumentWithFile(EmployeeDocumentModel request, IFormFile file);
        Task<List<EmployeeDocumentResponseModel>> GetPendingDocumentsByOrganization(int orgId);
        Task<PagedResult<EmployeeDocumentResponseModel>> GetPendingDocumentsByOrganization(int orgId, int page, int size, string? search);

        Task<bool> DeleteDocument(int id);
        Task<List<AdminDocRequirementResponseModel>> GetAdminDocSetup(int orgId);
        Task<bool> UpdateOrganizationRequirements(int orgId, List<int> selectedDocIds);
        Task<PagedResult<EmployeeDocumentSummaryResponseModel>> GetEmployeeDocumentSummaryBySearch(
            int orgId,
            int page,
            int size,
            string? searchTerm,
            bool? hasUploadedDocuments);
    }
}
