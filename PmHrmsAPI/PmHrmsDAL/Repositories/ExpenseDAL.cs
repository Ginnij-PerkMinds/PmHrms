using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class ExpenseDAL
    {
        private readonly PmHrmsContext _context;
        public ExpenseDAL(PmHrmsContext context) { _context = context; }

        public async Task<ExpenseMaster?> GetMasterType(int id) => await _context.ExpenseMasters.FindAsync(id);

        public async Task<OrganizationExpenseConfig?> GetOrgConfig(int orgId, int typeId) =>
            await _context.OrganizationExpenseConfigs.FirstOrDefaultAsync(x => x.OrganizationId == orgId && x.ExpenseTypeId == typeId);

        public async Task AddClaim(ExpenseClaim claim)
        {
            await _context.ExpenseClaims.AddAsync(claim);
            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetFullConfigForOrg(int orgId)
        {
            var masters = await _context.ExpenseMasters.Where(m => m.IsActive).ToListAsync();
            var configs = await _context.OrganizationExpenseConfigs.Where(c => c.OrganizationId == orgId).ToListAsync();

            return masters.Select(m => {
                var c = configs.FirstOrDefault(x => x.ExpenseTypeId == m.Id);
                return (object)new
                {
                    ExpenseTypeId = m.Id,
                    TypeName = m.TypeName,
                    GlobalLimit = m.DefaultMaxLimit,
                    OrgLimit = c?.MaxLimit,
                    IsEnabled = c?.IsEnabled ?? true
                };
            }).ToList();
        }

        public async Task AddOrgConfig(OrganizationExpenseConfig config)
        {
            await _context.OrganizationExpenseConfigs.AddAsync(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrgConfig(OrganizationExpenseConfig config)
        {
            _context.OrganizationExpenseConfigs.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExpenseClaim>> GetClaimsByEmp(int empId) =>
            await _context.ExpenseClaims.Include(x => x.ExpenseType).Where(x => x.UserId == empId).ToListAsync();

        public async Task<List<ExpenseClaim>> GetOrgClaims(int orgId, string? status)
        {
            var q = _context.ExpenseClaims.Include(x => x.ExpenseType).Include(x => x.User).Where(x => x.OrganizationId == orgId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
            return await q.ToListAsync();
        }

        public async Task<ExpenseClaim?> GetClaimById(int id) => await _context.ExpenseClaims.FindAsync(id);

        public async Task UpdateClaimStatus(ExpenseClaim claim)
        {
            _context.ExpenseClaims.Update(claim);
            await _context.SaveChangesAsync();
        }
    }
}