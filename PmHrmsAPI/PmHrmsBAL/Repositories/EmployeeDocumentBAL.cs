using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using PmHrmsAPI.PmHrmsFAL.IRepositories;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class EmployeeDocumentBAL : IEmployeeDocumentBAL
    {
        private readonly EmployeeDocumentDAL _docDAL;
        private readonly IDocumentFAL _documentFAL;
        private readonly IPermissionService _permissionService;

        public EmployeeDocumentBAL(EmployeeDocumentDAL docDAL, IDocumentFAL documentFAL, IPermissionService permissionService)
        {
            _docDAL = docDAL;
            _documentFAL = documentFAL;
            _permissionService = permissionService;
        }

        public async Task<PagedResult<EmployeeDocumentResponseModel>> GetAllDocuments(
            int page,
            int size,
            string? search,
            int? employeeId,
            int orgId)
        {
            _permissionService.Ensure(PermissionKeys.EMP_DOC_VERIFY);

            var (docs, totalCount) = await _docDAL.GetAllDocuments(page, size, search, employeeId, orgId);
            var items = docs.Select(MapEmployeeDocument).ToList();

            return new PagedResult<EmployeeDocumentResponseModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        public async Task<List<EmployeeDocumentResponseModel>> GetDocumentsByEmployeeId(int employeeId)
        {
           
                if (_permissionService.IsSelf(employeeId))
                {
                    
                }
                else
                {
                    _permissionService.Ensure(PermissionKeys.EMP_PROFILE_VIEW);
                }

            var docs = await _docDAL.GetDocumentsByEmployeeId(employeeId);
            return docs.Select(MapEmployeeDocument).ToList();
        }

        public async Task<bool> AddDocument(EmployeeDocumentModel request)
        {
           // _permissionService.Ensure(PermissionKeys.EMP_DOC_UPLOAD);
             if (!_permissionService.IsSelf(request.EmployeeId))
                {
                    _permissionService.Ensure(PermissionKeys.EMP_DOC_UPLOAD);
                }

            var entity = new EmployeeDocument
            {
                EmployeeId = request.EmployeeId,
                DocumentMasterId = request.DocumentMasterId,
                DocumentType = request.DocumentType,
                DocumentPath = request.DocumentPath,
                ExpiryDate = request.ExpiryDate,
                UploadDate = DateTime.Now,
                //VerificationStatus = "Pending"
                VerificationStatus = DocumentStatus.Pending.ToString()
            };

            await _docDAL.AddDocument(entity);
            return true;
        }

        public async Task<bool> VerifyDocument(DocumentVerificationModel request)
        {
            var document = await _docDAL.GetDocumentById(request.DocumentId);
            if (document == null)
                return false;

            _permissionService.EnsureCanActOn(PermissionKeys.EMP_DOC_VERIFY, document.EmployeeId);

            //_permissionService.Ensure(PermissionKeys.EMP_DOC_VERIFY);

            if (!Enum.TryParse<DocumentStatus>(request.VerificationStatus, true, out var status))
                //throw new Exception("Invalid status");
                throw new Exception(PmHrmsConstants.EmployeeDocumentMessages.InvalidStatus);

            if (status == DocumentStatus.Pending)
                //throw new Exception("Cannot verify as Pending");
                throw new Exception(PmHrmsConstants.EmployeeDocumentMessages.CannotVerifyPending);

            int verifiedById = _permissionService.GetCurrentEmployeeId();

            return await _docDAL.VerifyDocument(
                request.DocumentId,
                status.ToString(),
                verifiedById,
                request.HrRemarks
            );
        }

        public async Task<List<OrganizationDocumentRequirementResponseModel>> GetRequirementsByOrgId(int orgId)
        {
            
            var requirements = await _docDAL.GetRequirementsByOrgId(orgId);

            return requirements.Select(r => new OrganizationDocumentRequirementResponseModel
            {
                RequirementId = r.RequirementId,
                OrganizationId = r.OrganizationId,
                DocumentMasterId = r.DocumentMasterId,
                DocumentType = r.DocumentType,
                IsMandatory = r.IsMandatory,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<bool> AddDocumentWithFile(EmployeeDocumentModel request, IFormFile file)
        {
             if (!_permissionService.IsSelf(request.EmployeeId))
                {
                    _permissionService.Ensure(PermissionKeys.EMP_DOC_UPLOAD);
                }
            //var path = await _documentFAL.UploadDocumentAsync(file, "EmployeeDocumentsPath");
            var path = await _documentFAL.UploadDocumentAsync(file, PmHrmsConstants.FolderNames.Documents);
            request.DocumentPath = path;
            return await AddDocument(request);
        }

        public async Task<bool> ReuploadDocument(int docId, EmployeeDocumentModel request, IFormFile file)
        {
            var existing = await _docDAL.GetDocumentById(docId);
            if (existing == null) return false;

            if (!_permissionService.IsSelf(existing.EmployeeId))
                {
                    _permissionService.Ensure(PermissionKeys.EMP_DOC_UPLOAD);
                }
            
            

            //var newPath = await _documentFAL.UploadDocumentAsync(file, "EmployeeDocumentsPath");
            var newPath = await _documentFAL.UploadDocumentAsync(file, PmHrmsConstants.FolderNames.Documents);
            _documentFAL.DeleteDocument(existing.DocumentPath);

            var result = await _docDAL.UpdateDocumentFile(
                docId,
                newPath,
                request.DocumentType,
                request.ExpiryDate,
                request.DocumentMasterId
            );

            return result != null;
        }

        public async Task<bool> DeleteDocument(int id)
        {
            // Documents delete permission have EMP_DOC_UPLOAD so...
            var existing = await _docDAL.GetDocumentById(id);
            if (existing == null) return false;

            if (!_permissionService.IsSelf(existing.EmployeeId))
            {
                _permissionService.Ensure(PermissionKeys.EMP_DOC_UPLOAD);
            }

            _documentFAL.DeleteDocument(existing.DocumentPath);
            return await _docDAL.DeleteDocument(id);
        }

        public async Task<List<AdminDocRequirementResponseModel>> GetAdminDocSetup(int orgId)
        {
            _permissionService.Ensure(PermissionKeys.DOC_CONFIG_MANAGE);
            var masterDocs = await _docDAL.GetAllMasterDocuments();
            var currentReqs = await _docDAL.GetRequirementsByOrgId(orgId);

            return masterDocs.Select(m => new AdminDocRequirementResponseModel
            {
                DocumentMasterId = m.DocumentId,
                DisplayName = m.DisplayName,
                IsSelected = currentReqs.Any(r => r.DocumentMasterId == m.DocumentId),
                IsMandatory = currentReqs.FirstOrDefault(r => r.DocumentMasterId == m.DocumentId)?.IsMandatory ?? false
            }).ToList();
        }

        public async Task<List<EmployeeDocumentResponseModel>> GetPendingDocumentsByOrganization(int orgId)
        {
            _permissionService.Ensure(PermissionKeys.EMP_DOC_VERIFY);
            var docs = await _docDAL.GetPendingDocumentsByOrganization(orgId);

            return docs.Select(MapEmployeeDocument).ToList();
        }

        public async Task<PagedResult<EmployeeDocumentResponseModel>> GetPendingDocumentsByOrganization(
            int orgId,
            int page,
            int size,
            string? search)
        {
            _permissionService.Ensure(PermissionKeys.EMP_DOC_VERIFY);
            var (docs, totalCount) = await _docDAL.GetPendingDocumentsByOrganization(orgId, page, size, search);
            var items = docs.Select(MapEmployeeDocument).ToList();

            return new PagedResult<EmployeeDocumentResponseModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        public async Task<bool> UpdateOrganizationRequirements(int orgId, List<int> selectedDocIds)
        {
            _permissionService.Ensure(PermissionKeys.DOC_CONFIG_MANAGE);
            return await _docDAL.UpdateOrganizationRequirements(orgId, selectedDocIds);
        }

        public async Task<PagedResult<EmployeeDocumentSummaryResponseModel>> GetEmployeeDocumentSummaryBySearch(
            int orgId,
            int page,
            int size,
            string? searchTerm,
            bool? hasUploadedDocuments)
        {
            _permissionService.Ensure(PermissionKeys.EMP_DOC_VERIFY);

            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var (items, totalCount) = await _docDAL.GetEmployeeDocumentSummaryBySearch(
                orgId,
                page,
                size,
                searchTerm,
                hasUploadedDocuments);

            return new PagedResult<EmployeeDocumentSummaryResponseModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }

        private static EmployeeDocumentResponseModel MapEmployeeDocument(EmployeeDocument d)
        {
            return new EmployeeDocumentResponseModel
            {
                DocumentId = d.DocumentId,
                EmployeeId = d.EmployeeId,
                EmployeeCode = d.Employee?.EmployeeCode,
                EmployeeName = $"{d.Employee?.FirstName} {d.Employee?.LastName}".Trim(),
                DocumentMasterId = d.DocumentMasterId,
                DocumentDisplayName = d.DocumentMaster?.DisplayName,
                DocumentType = d.DocumentType,
                DocumentPath = d.DocumentPath,
                UploadDate = d.UploadDate,
                ExpiryDate = d.ExpiryDate,
                VerificationStatus = d.VerificationStatus,
                HrRemarks = d.HrRemarks,
                VerifiedById = d.VerifiedById,
                VerifiedByName = d.VerifiedBy != null ? $"{d.VerifiedBy.FirstName} {d.VerifiedBy.LastName}" : null,
                VerifiedDate = d.VerifiedDate
            };
        }
    }
}
