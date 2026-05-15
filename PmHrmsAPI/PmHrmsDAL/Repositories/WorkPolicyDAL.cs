using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class WorkPolicyDAL
    {
        private readonly PmHrmsContext _context;

        public WorkPolicyDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<(List<WorkPolicy>, int totalCount)> GetAll(
         int page,
         int size,
         string? search,
         int orgId)
            {
            var query = _context.WorkPolicies
                .Include(x => x.WeekOffs)
                .Where(x => x.OrganizationId == orgId && x.IsActive)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.PolicyName.Contains(search));
            }

            int count = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (data, count);
        }

        public async Task<WorkPolicy?> GetById(int id)
        {
            return await _context.WorkPolicies
                .Include(x => x.WeekOffs)
                .FirstOrDefaultAsync(x => x.PolicyId == id);
        }

        public async Task<WorkPolicy> Add(WorkPolicy entity)
        {
            await _context.WorkPolicies.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<WorkPolicy?> Update(WorkPolicy entity)
        {
            var existing = await _context.WorkPolicies.FindAsync(entity.PolicyId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _context.WorkPolicies.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = false;   // soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Employee?> GetEmployeeById(int employeeId)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<WorkPolicyResult?> GetEmployeePolicy(int policyId)
        {
            var empPolicy = await _context.WorkPolicies.FirstOrDefaultAsync(p => p.PolicyId == policyId && p.IsActive);
            return empPolicy == null ? null : new WorkPolicyResult { Policy = empPolicy, Source = WorkPolicySource.EMPLOYEE_LEVEL };
        }

        public async Task<WorkPolicyResult?> GetDesignationPolicy(int designationId)
        {
            var mapping = await _context.DesignationWorkPolicyMappings.FirstOrDefaultAsync(m => m.DesignationId == designationId);
            if (mapping == null) return null;

            var desigPolicy = await _context.WorkPolicies.FirstOrDefaultAsync(p => p.PolicyId == mapping.WorkPolicyId && p.IsActive);
            return desigPolicy == null ? null : new WorkPolicyResult { Policy = desigPolicy, Source = WorkPolicySource.DESIGNATION_LEVEL };
        }

        public async Task<WorkPolicyResult?> GetDefaultPolicy(int orgId)
        {
            var defaultPolicy = await _context.WorkPolicies
                .Where(p => p.OrganizationId == orgId && p.IsDefault && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return defaultPolicy == null ? null : new WorkPolicyResult { Policy = defaultPolicy, Source = WorkPolicySource.DEFAULT };
        }

        public async Task<bool> DesignationExists(int designationId)
        {
            return await _context.Designations.AnyAsync(d => d.DesignationId == designationId);
        }

        public async Task UnsetDefaultPolicies(int orgId)
        {
            var existingDefaults = await _context.WorkPolicies.Where(x => x.OrganizationId == orgId && x.IsDefault).ToListAsync();
            foreach (var policy in existingDefaults) policy.IsDefault = false;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMapping(int designationId, int policyId)
        {
            var mapping = await _context.DesignationWorkPolicyMappings
                .FirstOrDefaultAsync(x => x.DesignationId == designationId && x.WorkPolicyId == policyId);
            if (mapping != null)
            {
                _context.DesignationWorkPolicyMappings.Remove(mapping);
                await _context.SaveChangesAsync();
            }
        }

        public void SyncWeekOffs(WorkPolicy entity, IEnumerable<WorkPolicyWeekOffModel>? weekOffs)
        {
            if (entity.WeekOffs.Any())
            {
                _context.WorkPolicyWeekOffs.RemoveRange(entity.WeekOffs.ToList());
                entity.WeekOffs.Clear();
            }

            //foreach (var weekOff in BuildWeekOffEntities(weekOffs, entity.PolicyId))
            //{
            //    entity.WeekOffs.Add(weekOff);
            //}
        }

        public async Task<List<DesignationPolicyMappingResponse>> GetDesignationMappings()
        {
            return await (
                from m in _context.DesignationWorkPolicyMappings
                join d in _context.Designations
                    on m.DesignationId equals d.DesignationId
                join p in _context.WorkPolicies
                    on m.WorkPolicyId equals p.PolicyId
                where d.IsActive && p.IsActive
                select new DesignationPolicyMappingResponse
                {
                    DesignationId = d.DesignationId,
                    DesignationName = d.DesignationName,
                    PolicyName = p.PolicyName
                }
            ).ToListAsync();
        }



    }
}
