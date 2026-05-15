using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class DesignationDAL
    {
        private readonly PmHrmsContext _context;

        public DesignationDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<(List<Designation>, int totalCount)> GetAllDesignations(int page, int size, string? search, int orgId)
        {
            var query = _context.Designations
                                 .Where(d => d.IsActive && d.Department.OrganizationId == orgId)         // ACTIVE DESIGNATIONS ONLY
                                .Include(d => d.Department)             
                                .ThenInclude(dept => dept.Organization) 
                                .AsNoTracking()
                                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.DesignationName.Contains(search));
            }

            int count = await query.CountAsync();

            var data = await query.OrderByDescending(d => d.DesignationId)
                                  .Skip((page - 1) * size)
                                  .Take(size)
                                  .ToListAsync();

            return (data, count);
        }

        public async Task<List<Designation>> GetByDepartment(int departmentId, int orgId)
        {
            return await _context.Designations
                .Where(d =>
                    d.DepartmentId == departmentId &&
                    d.IsActive)
                .ToListAsync();
        }



        public async Task<Designation?> GetDesignation(int id  , int orgId)
        {
            return await _context.Designations
                                .Where(d => d.DesignationId == id &&
                                    d.Department.OrganizationId == orgId)
                                 .Include(d => d.Department)
                                 .ThenInclude(dept => dept.Organization)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
        }

        public async Task<Designation> AddDesignation(Designation designation)
        {
            await _context.Designations.AddAsync(designation);
            await _context.SaveChangesAsync();
            return designation;
        }
        public async Task<string?> GetWorkPolicyByDesignationId(int designationId, int orgId)
        {
            var mapping = await _context.DesignationWorkPolicyMappings
                .FirstOrDefaultAsync(x => x.DesignationId == designationId);

          
            if (mapping != null)
            {
                var policy = await _context.WorkPolicies
                    .FirstOrDefaultAsync(p => p.PolicyId == mapping.WorkPolicyId);

                if (policy != null)
                    return policy.PolicyName;
            }

            
            var defaultPolicy = await _context.WorkPolicies
                .Where(p => p.OrganizationId == orgId && p.IsDefault && p.IsActive)
                .FirstOrDefaultAsync();

            return defaultPolicy?.PolicyName;
        }
        public async Task<Department?> GetDefaultDepartment(int orgId)
        {
            return await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.OrganizationId == orgId &&
                    d.IsSystemDefault
                );
        }


        public async Task<bool> IsDepartmentBelongsToOrg(int deptId, int orgId)
        {
            return await _context.Departments
                .AnyAsync(d => d.DepartmentId == deptId && d.OrganizationId == orgId);
        }


        


        public async Task<int> GetNextHierarchyLevel(int departmentId)
        {
            var max = await _context.Designations
                .Where(d => d.DepartmentId == departmentId)
                .MaxAsync(d => (int?)d.HierarchyLevel);

            return (max ?? 0) + 1;
        }

        public async Task<Designation?> UpdateDesignation(Designation designation)
        {
            var existing = await _context.Designations.FindAsync(designation.DesignationId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(designation);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteDesignation(int id)
        {
            var desig = await _context.Designations.FindAsync(id);
            if (desig == null) return false;

           
           desig.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesignationExists(
    string name,
    int departmentId)
        {
            return await _context.Designations
                .AnyAsync(d =>
                    d.DepartmentId == departmentId &&
                    d.IsActive &&
                    d.DesignationName.ToLower() == name.ToLower());
        }

        public async Task<bool> DesignationExistsForUpdate(
            string name,
            int departmentId,
            int excludeId)
        {
            return await _context.Designations
                .AnyAsync(d =>
                    d.DesignationId != excludeId &&
                    d.DepartmentId == departmentId &&
                    d.IsActive &&
                    d.DesignationName.ToLower() == name.ToLower());
        }

        // Duplicate Hierarchy Check
        public async Task<bool> HierarchyExists(
            int departmentId,
            int hierarchyLevel)
        {
            return await _context.Designations
                .AnyAsync(d =>
                    d.DepartmentId == departmentId &&
                    d.IsActive &&
                    d.HierarchyLevel == hierarchyLevel);
        }

        public async Task<bool> HierarchyExistsForUpdate(
            int departmentId,
            int hierarchyLevel,
            int excludeId)
        {
            return await _context.Designations
                .AnyAsync(d =>
                    d.DesignationId != excludeId &&
                    d.DepartmentId == departmentId &&
                    d.IsActive &&
                    d.HierarchyLevel == hierarchyLevel);
        }


    }
}