using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDocumentController : ControllerBase
    {
        private readonly IEmployeeDocumentBAL _docBAL;

        public EmployeeDocumentController(IEmployeeDocumentBAL docBAL)
        {
            _docBAL = docBAL;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? employeeId = null)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;

            var result = await _docBAL.GetAllDocuments(pageNumber, pageSize, searchTerm, employeeId, claimOrgId);
            return Ok(new ApiResponseModel<PagedResult<EmployeeDocumentResponseModel>>(true, "Success", result));
        }

        [HttpGet("ByEmployee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var result = await _docBAL.GetDocumentsByEmployeeId(employeeId);
            return Ok(new ApiResponseModel<List<EmployeeDocumentResponseModel>>(true, "Success", result));
        }

        [HttpGet("SearchEmployeeStatus/{orgId}")]
        public async Task<IActionResult> SearchEmployeeStatus(
            int orgId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? submissionFilter = null)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;
            if (orgId != 0 && orgId != claimOrgId)
                return Forbid();

            bool? hasUploadedDocuments = submissionFilter?.Trim().ToUpperInvariant() switch
            {
                "UPLOADED" => true,
                "NOT_SUBMITTED" => false,
                _ => null
            };

            var result = await _docBAL.GetEmployeeDocumentSummaryBySearch(
                claimOrgId,
                pageNumber,
                pageSize,
                searchTerm,
                hasUploadedDocuments);

            return Ok(new ApiResponseModel<PagedResult<EmployeeDocumentSummaryResponseModel>>(true, "Success", result));
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] EmployeeDocumentModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _docBAL.AddDocument(model);
            return Ok(new ApiResponseModel<string>(true, "Document Record Added", null));
        }


        [HttpPost("Verify")]
        public async Task<IActionResult> Verify([FromBody] DocumentVerificationModel model)
        {
            var result = await _docBAL.VerifyDocument(model);
            if (!result) return NotFound(new ApiResponseModel<string>(false, "Document not found", null));

            return Ok(new ApiResponseModel<string>(true, "Document Examined", null));
        }

        [HttpGet("RequirementsByOrganization/{orgId}")]
        public async Task<IActionResult> GetRequirementsByOrgId(int orgId)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;
            if (orgId != 0 && orgId != claimOrgId)
                return Forbid();

            var result = await _docBAL.GetRequirementsByOrgId(claimOrgId);

            if (result == null || result.Count == 0)
                return NotFound(new ApiResponseModel<string>(false, "No requirements found for this organization", null));

            return Ok(new ApiResponseModel<List<OrganizationDocumentRequirementResponseModel>>(true, "Success", result));
        }


        [HttpGet("PublicRequirements/{orgId}")]  
        public async Task<IActionResult> GetPublicRequirements(int orgId)
        {
            var result = await _docBAL.GetRequirementsByOrgId(orgId);
            return Ok(new ApiResponseModel<List<OrganizationDocumentRequirementResponseModel>>(true, "Success", result));
        }


        [HttpGet("PendingByOrganization/{orgId}")]
        public async Task<IActionResult> GetPendingByOrganization(
            int orgId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;
            if (orgId != 0 && orgId != claimOrgId)
                return Forbid();

            var result = await _docBAL.GetPendingDocumentsByOrganization(claimOrgId, pageNumber, pageSize, searchTerm);

            return Ok(new ApiResponseModel<PagedResult<EmployeeDocumentResponseModel>>(
                true,
                "Success",
                result
            ));
        }



        [HttpPut("Reupload/{documentId}")]
        public async Task<IActionResult> Reupload(
      int documentId,
      [FromForm] ReuploadDocumentRequest request)
        {
            var model = new EmployeeDocumentModel
            {
                EmployeeId = request.EmployeeId,
                DocumentMasterId = request.DocumentMasterId,
                DocumentType = request.DocumentType,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _docBAL.ReuploadDocument(documentId, model, request.File);

            return Ok(result);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _docBAL.DeleteDocument(id);
            if (!result) return NotFound(new ApiResponseModel<string>(false, "Not Found", null));
            return Ok(new ApiResponseModel<string>(true, "Deleted", null));
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm] ReuploadDocumentRequest request)
        {
            var model = new EmployeeDocumentModel
            {
                EmployeeId = request.EmployeeId,
                DocumentMasterId = request.DocumentMasterId,
                DocumentType = request.DocumentType,
                ExpiryDate = request.ExpiryDate
            };

            var result = await _docBAL.AddDocumentWithFile(model, request.File);

            return Ok(result);
        }




        [HttpGet("Admin/SetupList/{orgId}")]
        public async Task<IActionResult> GetAdminSetupList(int orgId)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;
            if (orgId != 0 && orgId != claimOrgId)
                return Forbid();

            var result = await _docBAL.GetAdminDocSetup(claimOrgId);
            return Ok(new ApiResponseModel<List<AdminDocRequirementResponseModel>>(true, "Success", result));
        }

        [HttpPost("Admin/UpdateRequirements/{orgId}")]
        public async Task<IActionResult> UpdateRequirements(int orgId, [FromBody] List<int> docIds)
        {
            if (!TryGetClaimOrgId(out var claimOrgId, out var unauthorized))
                return unauthorized!;
            if (orgId != 0 && orgId != claimOrgId)
                return Forbid();

            var result = await _docBAL.UpdateOrganizationRequirements(claimOrgId, docIds);
            return Ok(new ApiResponseModel<bool>(true, "Requirements updated successfully", result));
        }

        private bool TryGetClaimOrgId(out int orgId, out IActionResult? unauthorized)
        {
            unauthorized = null;
            orgId = 0;

            var orgClaim = User.FindFirst("OrgId")?.Value;
            if (!int.TryParse(orgClaim, out orgId) || orgId <= 0)
            {
                unauthorized = Unauthorized(new ApiResponseModel<string>(false, "Invalid token claims", null));
                return false;
            }

            return true;
        }
    }
}
    
