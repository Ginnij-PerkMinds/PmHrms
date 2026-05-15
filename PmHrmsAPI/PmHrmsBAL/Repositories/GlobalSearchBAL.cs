using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using System.Diagnostics;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class GlobalSearchBAL : IGlobalSearchBAL
    {
        private readonly GlobalSearchDAL _globalSearchDAL;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<GlobalSearchBAL> _logger;

        public GlobalSearchBAL(
            GlobalSearchDAL globalSearchDAL,
            IPermissionService permissionService,
            ILogger<GlobalSearchBAL> logger)
        {
            _globalSearchDAL = globalSearchDAL;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<GlobalSearchResponseModel> Search(string searchTerm, int orgId, int limit, string scope)
        {
            var sw = Stopwatch.StartNew();
            var normalizedScope = NormalizeScope(scope);
            _logger.LogInformation(
                "GlobalSearchBAL started. OrgId: {OrgId}, RawLimit: {Limit}, Scope: {Scope}, SearchTermLength: {SearchTermLength}",
                orgId,
                limit,
                normalizedScope,
                searchTerm?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogInformation("GlobalSearchBAL empty search term. Returning empty response. OrgId: {OrgId}", orgId);
                return new GlobalSearchResponseModel();
            }

            var safeLimit = Math.Clamp(limit, 1, 20);
            var term = searchTerm.Trim();

            var canSearchEmployees =
                _permissionService.Has(PermissionKeys.EMP_VIEW) ||
                _permissionService.Has(PermissionKeys.EMP_PROFILE_VIEW);
            var canSearchDepartments = _permissionService.Has(PermissionKeys.DEPT_VIEW);
            var canSearchDesignations = _permissionService.Has(PermissionKeys.DESIG_VIEW);
            var canSearchDocuments = _permissionService.Has(PermissionKeys.EMP_DOC_VERIFY);

            _logger.LogInformation(
                "GlobalSearchBAL permissions evaluated. OrgId: {OrgId}, SafeLimit: {SafeLimit}, Scope: {Scope}, CanEmployees: {CanEmployees}, CanDepartments: {CanDepartments}, CanDesignations: {CanDesignations}, CanDocuments: {CanDocuments}",
                orgId,
                safeLimit,
                normalizedScope,
                canSearchEmployees,
                canSearchDepartments,
                canSearchDesignations,
                canSearchDocuments);

            var searchAll = normalizedScope == SearchScope.All;
            var employees = (searchAll || normalizedScope == SearchScope.Employees) && canSearchEmployees
                ? await _globalSearchDAL.SearchEmployees(orgId, term, safeLimit)
                : new List<GlobalSearchEmployeeResponseModel>();

            var departments = (searchAll || normalizedScope == SearchScope.Departments) && canSearchDepartments
                ? await _globalSearchDAL.SearchDepartments(orgId, term, safeLimit)
                : new List<GlobalSearchDepartmentResponseModel>();

            var designations = (searchAll || normalizedScope == SearchScope.Designations) && canSearchDesignations
                ? await _globalSearchDAL.SearchDesignations(orgId, term, safeLimit)
                : new List<GlobalSearchDesignationResponseModel>();

            var documents = (searchAll || normalizedScope == SearchScope.Documents) && canSearchDocuments
                ? await _globalSearchDAL.SearchDocuments(orgId, term, safeLimit)
                : new List<GlobalSearchDocumentResponseModel>();

            var result = new GlobalSearchResponseModel
            {
                Employees = employees,
                Departments = departments,
                Designations = designations,
                Documents = documents
            };

            result.Total =
                result.Employees.Count +
                result.Departments.Count +
                result.Designations.Count +
                result.Documents.Count;

            sw.Stop();
            _logger.LogInformation(
                "GlobalSearchBAL completed. OrgId: {OrgId}, Total: {Total}, Employees: {Employees}, Departments: {Departments}, Designations: {Designations}, Documents: {Documents}, DurationMs: {DurationMs}",
                orgId,
                result.Total,
                result.Employees.Count,
                result.Departments.Count,
                result.Designations.Count,
                result.Documents.Count,
                sw.ElapsedMilliseconds);

            return result;
        }

        private static SearchScope NormalizeScope(string? scope)
        {
            return scope?.Trim().ToLowerInvariant() switch
            {
                //"employee" or "employees" => SearchScope.Employees,
                PmHrmsConstants.GlobalSearchScopes.EmployeeSingular or PmHrmsConstants.GlobalSearchScopes.EmployeePlural => SearchScope.Employees,
                //"department" or "departments" => SearchScope.Departments,
                PmHrmsConstants.GlobalSearchScopes.DepartmentSingular or PmHrmsConstants.GlobalSearchScopes.DepartmentPlural => SearchScope.Departments,
                //"designation" or "designations" => SearchScope.Designations,
                PmHrmsConstants.GlobalSearchScopes.DesignationSingular or PmHrmsConstants.GlobalSearchScopes.DesignationPlural => SearchScope.Designations,
                //"document" or "documents" => SearchScope.Documents,
                PmHrmsConstants.GlobalSearchScopes.DocumentSingular or PmHrmsConstants.GlobalSearchScopes.DocumentPlural => SearchScope.Documents,
                _ => SearchScope.All
            };
        }

        private enum SearchScope
        {
            All,
            Employees,
            Departments,
            Designations,
            Documents
        }
    }
}
