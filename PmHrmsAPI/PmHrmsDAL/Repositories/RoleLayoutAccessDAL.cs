using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class RoleLayoutAccessDAL
    {
        private readonly PmHrmsContext _context;

        public RoleLayoutAccessDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<string?> GetDefaultLayoutKey(int systemRoleId)
        {
            return await _context.RoleLayoutAccesses
                .Include(x => x.Layout)
                .Where(x => x.SystemRoleId == systemRoleId && x.IsAllowed)
                .Select(x => x.Layout.LayoutKey)
                .FirstOrDefaultAsync();
        }

        // 🔥 All layout access
        public async Task<List<RoleLayoutAccess>> GetRoleLayoutAccess(int systemRoleId)
        {
            return await _context.RoleLayoutAccesses
                .Include(x => x.Layout)
                .Include(x => x.SystemRole)
                .Where(x => x.SystemRoleId == systemRoleId)
                .AsNoTracking()
                .ToListAsync();
        }

        // 🔥 UPSERT
        public async Task<RoleLayoutAccess> Upsert(RoleLayoutAccess entity)
        {
            var existing = await _context.RoleLayoutAccesses
                .FirstOrDefaultAsync(x =>
                    x.SystemRoleId == entity.SystemRoleId &&
                    x.LayoutId == entity.LayoutId);

            if (existing != null)
            {
                existing.IsAllowed = entity.IsAllowed;
            }
            else
            {
                _context.RoleLayoutAccesses.Add(entity);
            }

            await _context.SaveChangesAsync();
            return entity;
        }
    }
}