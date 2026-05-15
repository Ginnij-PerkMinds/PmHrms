using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class HolidayDAL
    {
        private readonly PmHrmsContext _context;

        public HolidayDAL(PmHrmsContext context)
        {
            _context = context;
        }

        // ── MASTER CATALOG ────────────────────────────────────────────────────

        // Global holidays + Custom holidays made by this specific Org
        public async Task<List<SystemHoliday>> GetMasterHolidaysForOrg(int orgId, int year)
        {
            return await _context.SystemHolidays
                .AsNoTracking()
                .Where(h => h.Year == year && (!h.IsCustom || h.CreatedByOrgId == orgId))
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();
        }


        public async Task<List<HolidayGroup>> GetActiveGroupsWithDetailsAsync(int orgId, int year)
        {
            return await _context.HolidayGroups
                .Include(g => g.GroupHolidays)
                    .ThenInclude(gh => gh.SystemHoliday)
                .Include(g => g.EligibilityRules)
                .AsSplitQuery()                              
                .Where(g =>
                    g.OrganizationId == orgId &&
                    g.Year           == year  &&
                    g.IsActive)
                .ToListAsync();
        }

        public async Task<SystemHoliday?> GetSystemHolidayById(int id)
        {
            return await _context.SystemHolidays.FindAsync(id);
        }

        public async Task AddSystemHoliday(SystemHoliday holiday)
        {
            await _context.SystemHolidays.AddAsync(holiday);
        }

        public void RemoveSystemHolidays(List<SystemHoliday> holidays)
        {
            _context.SystemHolidays.RemoveRange(holidays);
        }

        // ── HOLIDAY GROUPS (CALENDARS) ─────────────────────────────────────────

        // Get all groups with their holidays and eligibility rules
        public async Task<List<HolidayGroup>> GetHolidayGroupsByOrgAndYear(int orgId, int year)
        {
            return await _context.HolidayGroups
                .Include(g => g.GroupHolidays)
                    .ThenInclude(gh => gh.SystemHoliday)
                .Include(g => g.EligibilityRules)
                .AsSplitQuery()
                .Where(g => g.OrganizationId == orgId && g.Year == year)
                .ToListAsync();
        }

        // Get single group by id (for update/delete)
        public async Task<HolidayGroup?> GetHolidayGroupById(int groupId, int orgId)
        {
            return await _context.HolidayGroups
                .Include(g => g.GroupHolidays)
                .Include(g => g.EligibilityRules)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.OrganizationId == orgId);
        }

        public async Task AddHolidayGroup(HolidayGroup group)
        {
            await _context.HolidayGroups.AddAsync(group);
        }

        public async Task ClearDefaultHolidayGroups(int orgId, int year, int? excludeGroupId = null)
        {
            var query = _context.HolidayGroups
                .Where(g => g.OrganizationId == orgId &&
                            g.Year == year &&
                            g.IsDefault);

            if (excludeGroupId.HasValue)
            {
                query = query.Where(g => g.Id != excludeGroupId.Value);
            }

            await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(g => g.IsDefault, false));
        }

        // Clears child rows before re-inserting on update
        public void RemoveGroupMappingsAndRules(HolidayGroup group)
        {
            _context.HolidayGroupMappings.RemoveRange(group.GroupHolidays);
            _context.HolidayGroupEligibilities.RemoveRange(group.EligibilityRules);
        }

        public void RemoveHolidayGroup(HolidayGroup group)
        {
            _context.HolidayGroups.Remove(group);
        }

        public async Task<List<Employee>> GetEmployeesForHolidayGroupSync(int orgId)
        {
            return await _context.Employees
                .Include(e => e.HolidayGroup)
                .Where(e => e.OrganizationId == orgId)
                .ToListAsync();
        }

        public async Task ClearHolidayGroupAssignments(int orgId, int groupId)
        {
            await _context.Employees
                .Where(e => e.OrganizationId == orgId && e.HolidayGroupId == groupId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.HolidayGroupId, (int?)null));
        }

        // ── GROUP MAPPINGS ─────────────────────────────────────────────────────

        // Checks if a custom SystemHoliday is referenced in any group of this org
        // Used before hard-deleting a custom holiday to clean up orphan mappings
        public async Task<List<HolidayGroupMapping>> GetAssignmentsBySystemHolidayAndOrg(
            int orgId, int systemHolidayId)
        {
            return await _context.HolidayGroupMappings
                .Where(m =>
                    m.SystemHolidayId == systemHolidayId &&
                    m.HolidayGroup.OrganizationId == orgId)
                .ToListAsync();
        }

        public void RemoveGroupMappings(List<HolidayGroupMapping> mappings)
        {
            _context.HolidayGroupMappings.RemoveRange(mappings);
        }

        // ── SHARED ────────────────────────────────────────────────────────────

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
