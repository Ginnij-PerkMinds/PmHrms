using PmHrmsAPI.PmHrmsDAL.DbEntities;
using Microsoft.EntityFrameworkCore;  

using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class EmployeeDocumentDAL
    {
       
            private readonly PmHrmsContext _context;

            public EmployeeDocumentDAL(PmHrmsContext context)
            {
                _context = context;
            }

            public async Task<(List<EmployeeDocument>, int totalCount)> GetAllDocuments(
                int page,
                int size,
                string? search,
                int? employeeId,
                int orgId)
            {
                var query = BuildDocumentQuery()
                    .Where(d => d.Employee != null && d.Employee.OrganizationId == orgId);

                if (employeeId.HasValue)
                    query = query.Where(d => d.EmployeeId == employeeId.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();
                    query = query.Where(d =>
                        (d.Employee != null && (
                            (d.Employee.FirstName ?? "").Contains(search) ||
                            (d.Employee.LastName ?? "").Contains(search) ||
                            (d.Employee.EmployeeCode ?? "").Contains(search)
                        )) ||
                        (d.DocumentMaster != null && (d.DocumentMaster.DisplayName ?? "").Contains(search)) ||
                        (d.DocumentType ?? "").Contains(search) ||
                        (d.VerificationStatus ?? "").Contains(search));
                }

                int count = await query.CountAsync();

                var data = await query
                    .OrderByDescending(d => d.DocumentId)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return (data, count);
            }

         
            public async Task<List<EmployeeDocument>> GetDocumentsByEmployeeId(int employeeId)
            {
                return await BuildDocumentQuery()
                                     .Where(d => d.EmployeeId == employeeId)
                                     .ToListAsync();
            }

            public async Task<EmployeeDocument?> GetDocumentById(int id)
            {
                return await _context.EmployeeDocuments.FindAsync(id);
            }

            public async Task<EmployeeDocument> AddDocument(EmployeeDocument doc)
            {
                await _context.EmployeeDocuments.AddAsync(doc);
                await _context.SaveChangesAsync();
                return doc;
            }

            public async Task<bool> VerifyDocument(int docId, string status, int verifierId, string? remarks)
            {
                var doc = await _context.EmployeeDocuments.FindAsync(docId);
                if (doc == null) return false;

                doc.VerificationStatus = status;
                doc.VerifiedById = verifierId;
                doc.VerifiedDate = DateTime.Now;
                doc.HrRemarks = remarks;

                await _context.SaveChangesAsync();
                return true;
            }


        public async Task<List<OrganizationDocumentRequirement>> GetRequirementsByOrgId(int orgId)
        {
            return await _context.OrganizationDocumentRequirements
                                .Include(d => d.DocumentMaster)
                                 .Where(d => d.OrganizationId == orgId)
                                 .AsNoTracking()
                                 .ToListAsync();
        }




        public async Task<EmployeeDocument?> UpdateDocumentFile(int docId, string newPath, string? newType, DateOnly? expiryDate, int? documentMasterId)
        {
            var existingDoc = await _context.EmployeeDocuments.FindAsync(docId);
            if (existingDoc == null) return null;

            existingDoc.DocumentPath = newPath;
            if (documentMasterId.HasValue) existingDoc.DocumentMasterId = documentMasterId.Value; // Updated
            if (!string.IsNullOrEmpty(newType)) existingDoc.DocumentType = newType;
            if (expiryDate.HasValue) existingDoc.ExpiryDate = expiryDate;

            existingDoc.VerificationStatus = "Pending";
            existingDoc.UploadDate = DateTime.Now;
            existingDoc.VerifiedById = null;
            existingDoc.VerifiedDate = null;
            existingDoc.HrRemarks = null;

            await _context.SaveChangesAsync();
            return existingDoc;
        }


        public async Task<bool> DeleteDocument(int id)
            {
                var doc = await _context.EmployeeDocuments.FindAsync(id);
                if (doc == null) return false;
                _context.EmployeeDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                return true;
            }

        public async Task<List<DocumentMaster>> GetAllMasterDocuments()
        {
            return await _context.DocumentMasters.Where(d => d.IsActive == true).ToListAsync();
        }

        public async Task<List<EmployeeDocument>> GetPendingDocumentsByOrganization(int orgId)
        {
            return await BuildDocumentQuery()
                .Where(d =>
                    d.VerificationStatus == "Pending" &&
                    d.Employee != null &&
                    d.Employee.OrganizationId == orgId
                )
                .ToListAsync();
        }

        public async Task<(List<EmployeeDocument>, int totalCount)> GetPendingDocumentsByOrganization(
            int orgId,
            int page,
            int size,
            string? search)
        {
            var query = BuildDocumentQuery()
                .Where(d =>
                    d.VerificationStatus == "Pending" &&
                    d.Employee != null &&
                    d.Employee.OrganizationId == orgId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(d =>
                    (d.Employee != null && (
                        (d.Employee.FirstName ?? "").Contains(search) ||
                        (d.Employee.LastName ?? "").Contains(search) ||
                        (d.Employee.EmployeeCode ?? "").Contains(search)
                    )) ||
                    (d.DocumentMaster != null && (d.DocumentMaster.DisplayName ?? "").Contains(search)) ||
                    (d.DocumentType ?? "").Contains(search));
            }

            int count = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.DocumentId)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (data, count);
        }


        public async Task<bool> UpdateOrganizationRequirements(int orgId, List<int> selectedDocIds)
        {
            var oldReqs = _context.OrganizationDocumentRequirements.Where(r => r.OrganizationId == orgId);
            _context.OrganizationDocumentRequirements.RemoveRange(oldReqs);

            foreach (var docId in selectedDocIds)
            {
                var masterDoc = await _context.DocumentMasters.FindAsync(docId);
                if (masterDoc != null)
                {
                    _context.OrganizationDocumentRequirements.Add(new OrganizationDocumentRequirement
                    {
                        OrganizationId = orgId,
                        DocumentMasterId = docId,
                        DocumentType = masterDoc.DocumentKey,
                        IsMandatory = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<EmployeeDocumentSummaryResponseModel> items, int totalCount)> GetEmployeeDocumentSummaryBySearch(
            int orgId,
            int page,
            int size,
            string? searchTerm,
            bool? hasUploadedDocuments)
        {
            var term = searchTerm?.Trim();
            var requiredCount = await _context.OrganizationDocumentRequirements
                .CountAsync(r => r.OrganizationId == orgId);

            var employeeQuery = _context.Employees
                .Where(e =>
                    e.IsActive &&
                    e.OrganizationId == orgId &&
                    (string.IsNullOrWhiteSpace(term) ||
                     e.FirstName.Contains(term) ||
                     (e.LastName ?? "").Contains(term) ||
                     e.EmployeeCode.Contains(term)))
                .AsQueryable();

            if (hasUploadedDocuments.HasValue)
            {
                if (hasUploadedDocuments.Value)
                {
                    employeeQuery = employeeQuery.Where(e =>
                        _context.EmployeeDocuments.Any(d => d.EmployeeId == e.EmployeeId));
                }
                else
                {
                    employeeQuery = employeeQuery.Where(e =>
                        !_context.EmployeeDocuments.Any(d => d.EmployeeId == e.EmployeeId));
                }
            }

            var totalCount = await employeeQuery.CountAsync();

            var pagedEmployees = await employeeQuery
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ThenBy(e => e.EmployeeCode)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.EmployeeCode,
                    EmployeeName = ((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim()
                })
                .ToListAsync();

            if (!pagedEmployees.Any())
                return (new List<EmployeeDocumentSummaryResponseModel>(), totalCount);

            var pagedEmployeeIds = pagedEmployees.Select(e => e.EmployeeId).ToList();

            var aggregates = await _context.EmployeeDocuments
                .Where(d => pagedEmployeeIds.Contains(d.EmployeeId))
                .GroupBy(d => d.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    UploadedCount = g.Count(),
                    PendingCount = g.Count(x => (x.VerificationStatus ?? "") == "Pending"),
                    ApprovedCount = g.Count(x => (x.VerificationStatus ?? "") == "Approved"),
                    RejectedCount = g.Count(x => (x.VerificationStatus ?? "") == "Rejected")
                })
                .ToListAsync();

            var aggregateMap = aggregates.ToDictionary(x => x.EmployeeId, x => x);

            var items = pagedEmployees.Select(e =>
            {
                aggregateMap.TryGetValue(e.EmployeeId, out var agg);

                return new EmployeeDocumentSummaryResponseModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.EmployeeName,
                    RequiredCount = requiredCount,
                    UploadedCount = agg?.UploadedCount ?? 0,
                    PendingCount = agg?.PendingCount ?? 0,
                    ApprovedCount = agg?.ApprovedCount ?? 0,
                    RejectedCount = agg?.RejectedCount ?? 0
                };
            }).ToList();

            return (items, totalCount);
        }

        private IQueryable<EmployeeDocument> BuildDocumentQuery()
        {
            return _context.EmployeeDocuments
                .Include(d => d.Employee)
                .Include(d => d.VerifiedBy)
                .Include(d => d.DocumentMaster)
                .AsNoTracking()
                .AsQueryable();
        }
    }
    }
