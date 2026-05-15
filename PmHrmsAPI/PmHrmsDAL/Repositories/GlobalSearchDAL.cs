using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using System.Diagnostics;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class GlobalSearchDAL
    {
        private readonly PmHrmsContext _context;
        private readonly ILogger<GlobalSearchDAL> _logger;

        public GlobalSearchDAL(
            PmHrmsContext context,
            ILogger<GlobalSearchDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GlobalSearchEmployeeResponseModel>> SearchEmployees(
            int orgId,
            string searchTerm,
            int limit)
        {
            var sw = Stopwatch.StartNew();
            var term = searchTerm.Trim();

            var result = await _context.Employees
                .AsNoTracking()
                .Where(e =>
                    e.IsActive &&
                    e.OrganizationId == orgId &&
                    (
                        e.FirstName.Contains(term) ||
                        (e.LastName ?? "").Contains(term) ||
                        e.EmployeeCode.Contains(term) ||
                        e.OfficialEmail.Contains(term)
                    ))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ThenBy(e => e.EmployeeCode)
                .Select(e => new GlobalSearchEmployeeResponseModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FullName = ((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim(),
                    OfficialEmail = e.OfficialEmail,
                    PhoneNumber = e.PhoneNumber,
                    OrganizationId = e.OrganizationId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    DesignationName = e.Designation != null ? e.Designation.DesignationName : null
                })
                .Take(limit)
                .ToListAsync();

            sw.Stop();
            _logger.LogInformation(
                "GlobalSearchDAL SearchEmployees completed. OrgId: {OrgId}, Term: {Term}, Limit: {Limit}, Count: {Count}, DurationMs: {DurationMs}",
                orgId,
                term,
                limit,
                result.Count,
                sw.ElapsedMilliseconds);

            return result;
        }

        public async Task<List<GlobalSearchDepartmentResponseModel>> SearchDepartments(
            int orgId,
            string searchTerm,
            int limit)
        {
            var sw = Stopwatch.StartNew();
            var term = searchTerm.Trim();

            var result = await _context.Departments
                .AsNoTracking()
                .Where(d =>
                    d.IsActive &&
                    d.OrganizationId == orgId &&
                    d.DepartmentName.Contains(term))
                .OrderBy(d => d.DepartmentName)
                .Select(d => new GlobalSearchDepartmentResponseModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    OrganizationId = d.OrganizationId,
                    EmployeeCount = d.Employees.Count(e => e.IsActive)
                })
                .Take(limit)
                .ToListAsync();

            sw.Stop();
            _logger.LogInformation(
                "GlobalSearchDAL SearchDepartments completed. OrgId: {OrgId}, Term: {Term}, Limit: {Limit}, Count: {Count}, DurationMs: {DurationMs}",
                orgId,
                term,
                limit,
                result.Count,
                sw.ElapsedMilliseconds);

            return result;
        }

        public async Task<List<GlobalSearchDesignationResponseModel>> SearchDesignations(
            int orgId,
            string searchTerm,
            int limit)
        {
            var sw = Stopwatch.StartNew();
            var term = searchTerm.Trim();

            var result = await _context.Designations
                .AsNoTracking()
                .Where(d =>
                    d.IsActive &&
                    d.Department != null &&
                    d.Department.OrganizationId == orgId &&
                    d.DesignationName.Contains(term))
                .OrderBy(d => d.DesignationName)
                .ThenBy(d => d.HierarchyLevel)
                .Select(d => new GlobalSearchDesignationResponseModel
                {
                    DesignationId = d.DesignationId,
                    DesignationName = d.DesignationName,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department != null ? d.Department.DepartmentName : null,
                    HierarchyLevel = d.HierarchyLevel
                })
                .Take(limit)
                .ToListAsync();

            sw.Stop();
            _logger.LogInformation(
                "GlobalSearchDAL SearchDesignations completed. OrgId: {OrgId}, Term: {Term}, Limit: {Limit}, Count: {Count}, DurationMs: {DurationMs}",
                orgId,
                term,
                limit,
                result.Count,
                sw.ElapsedMilliseconds);

            return result;
        }

        public async Task<List<GlobalSearchDocumentResponseModel>> SearchDocuments(
            int orgId,
            string searchTerm,
            int limit)
        {
            var sw = Stopwatch.StartNew();
            var term = searchTerm.Trim();

            var result = await _context.EmployeeDocuments
                .AsNoTracking()
                .Include(d => d.Employee)
                .Include(d => d.DocumentMaster)
                .Where(d => 
                    d.Employee.IsActive &&
                    d.Employee.OrganizationId == orgId &&
                    (
                        d.Employee.FirstName.Contains(term) ||
                        d.Employee.LastName.Contains(term) ||
                        d.Employee.EmployeeCode.Contains(term) ||
                        (d.DocumentType ?? "").Contains(term) ||
                        (d.DocumentMaster != null && d.DocumentMaster.DisplayName.Contains(term))
                    ))
                .OrderBy(d => d.Employee.FirstName)
                .Select(d => new GlobalSearchDocumentResponseModel
                {
                    EmployeeId = d.EmployeeId,
                    EmployeeCode = d.Employee.EmployeeCode,
                    EmployeeName = ((d.Employee.FirstName ?? "") + " " + (d.Employee.LastName ?? "")).Trim(),
                    DocumentTypeName = d.DocumentMaster != null ? d.DocumentMaster.DisplayName : d.DocumentType,
                    UploadedCount = 1,
                    PendingCount = d.VerificationStatus == "Pending" ? 1 : 0,
                    ApprovedCount = d.VerificationStatus == "Approved" ? 1 : 0,
                    RejectedCount = d.VerificationStatus == "Rejected" ? 1 : 0
                })
                .Take(limit)
                .ToListAsync();

            sw.Stop();
            _logger.LogInformation("GlobalSearchDAL SearchDocuments (Detailed) completed. Count: {Count}", result.Count);

            return result;
        }
    }
}
