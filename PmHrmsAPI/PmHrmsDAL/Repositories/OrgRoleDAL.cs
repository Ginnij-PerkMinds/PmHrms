using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
   
    public class OrgRoleDAL
    {
        private readonly PmHrmsContext _context;

        public OrgRoleDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<List<OrgRole>> GetOrgRoles(int orgId)
        {
            return await _context.OrgRoles
                .Where(r => r.OrgId == orgId)
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SystemRole>> GetSystemRoles()
        {
            return await _context.SystemRoles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<OrgRole?> GetOrgRoleById(int id)
        {
            return await _context.OrgRoles.FindAsync(id);
        }

        public async Task<OrgRole> AddOrgRole(OrgRole role)
        {
            await _context.OrgRoles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<OrgRole?> UpdateOrgRole(OrgRole role)
        {
            var existing = await _context.OrgRoles.FindAsync(role.OrgRoleId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(role);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteOrgRole(int id)
        {
            var role = await _context.OrgRoles.FindAsync(id);
            if (role == null) return false;

            _context.OrgRoles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
