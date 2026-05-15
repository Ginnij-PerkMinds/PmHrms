using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class DepartmentDAL
    {
        private readonly PmHrmsContext _context;

        public DepartmentDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<(List<Department>, int totalCount)> GetAllDepartments(
            int page, int size, string? search)
        {
            var query = _context.Departments
                .Where(d => d.IsActive)
                .Include(d => d.Organization)
                .Include(d => d.Employees)
                .Include(d => d.Designations)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.DepartmentName.Contains(search));
            }

            int count = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.DepartmentId)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (data, count);
        }


        public async Task<Department?> GetDepartment(int id)
        {
            return await _context.Departments
                .Include(d => d.Organization)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
        }


        public async Task<bool> DepartmentExists(string name, int orgId)
        {
            name = name.Trim().ToLower();

            return await _context.Departments
                .AnyAsync(d =>
                    d.OrganizationId == orgId &&
                    d.IsActive &&
                    d.DepartmentName.ToLower() == name);
        }

      
        public async Task<bool> DepartmentExistsForUpdate(
            string name,
            int orgId,
            int departmentId)
        {
            name = name.Trim().ToLower();

            return await _context.Departments
                .AnyAsync(d =>
                    d.OrganizationId == orgId &&
                    d.IsActive &&
                    d.DepartmentId != departmentId &&
                    d.DepartmentName.ToLower() == name);
        }

       
        public async Task<Department> AddDepartment(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> UpdateDepartment(Department department)
        {
            var existing = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == department.DepartmentId);

            if (existing == null)
                return null;

            existing.DepartmentName = department.DepartmentName;
            existing.HeadOfDepartmentId = department.HeadOfDepartmentId;

            await _context.SaveChangesAsync();
            return existing;
        }

       
        public async Task<bool> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null)
                return false;

            dept.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
