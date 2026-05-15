using Microsoft.EntityFrameworkCore; 
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class DesignationWorkPolicyDAL
    {
        private readonly PmHrmsContext _context;

        public DesignationWorkPolicyDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<DesignationWorkPolicyMapping?> GetByDesignationId(int designationId)
        {
            return await _context.DesignationWorkPolicyMappings
                .FirstOrDefaultAsync(x => x.DesignationId == designationId);
        }

        public async Task AddOrUpdate(int designationId, int policyId)
        {
            var existing = await _context.DesignationWorkPolicyMappings
                .FirstOrDefaultAsync(x => x.DesignationId == designationId);

            if (existing != null)
            {
                existing.WorkPolicyId = policyId;
            }
            else
            {
                await _context.DesignationWorkPolicyMappings.AddAsync(
                    new DesignationWorkPolicyMapping
                    {
                        DesignationId = designationId,
                        WorkPolicyId = policyId
                    });
            }

            await _context.SaveChangesAsync();
        }
        // This method retrieves all active designation-policy mappings along with their names
        public async Task<List<DesignationPolicyMappingResponse>> GetMappings()
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

        public async Task RemoveMapping(int designationId, int policyId)
        {
            var mapping = await _context.DesignationWorkPolicyMappings
                .FirstOrDefaultAsync(x =>
                    x.DesignationId == designationId &&
                    x.WorkPolicyId == policyId);

            if (mapping != null)
            {
                _context.DesignationWorkPolicyMappings.Remove(mapping);
                await _context.SaveChangesAsync();
            }
        }
    }
}