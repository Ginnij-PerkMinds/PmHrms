using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsDAL.Repositories;

public class RolePermissionDAL
{
    private readonly PmHrmsContext _context;

    public RolePermissionDAL(PmHrmsContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionMaster>> GetAllPermissions()
    {
        return await _context.PermissionMasters.AsNoTracking().ToListAsync();
    }

    public async Task<List<RolePermission>> GetMappingsByRole(int orgRoleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.OrgRoleId == orgRoleId)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<bool> TransactionalSync(int orgRoleId, List<RolePermission> newMappings)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = _context.RolePermissions.Where(rp => rp.OrgRoleId == orgRoleId);
            _context.RolePermissions.RemoveRange(existing);

            await _context.RolePermissions.AddRangeAsync(newMappings);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<List<UserRoleAssignmentModel>> GetUsersWithRoles(int orgId)
    {
        return await _context.AppUsers
            .AsNoTracking()
            .Include(u => u.Employee)
            .Include(u => u.OrgRole)
            .Where(u => u.Employee.OrganizationId == orgId && u.Employee.IsActive)
            .Select(u => new UserRoleAssignmentModel
            {
                EmployeeId = u.EmployeeId,
                EmployeeCode = u.Employee.EmployeeCode,
                FullName = (u.Employee.FirstName + " " + (u.Employee.LastName ?? string.Empty)).Trim(),
                OfficialEmail = u.Employee.OfficialEmail,
                OrgRoleId = u.OrgRoleId,
                OrgRoleName = u.OrgRole != null ? u.OrgRole.Name : null
            })
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<bool> AssignRoleToUser(int orgId, int employeeId, int orgRoleId)
    {
        var roleExists = await _context.OrgRoles
            .AnyAsync(r => r.OrgRoleId == orgRoleId && r.OrgId == orgId);

        if (!roleExists)
        {
            return false;
        }

        var user = await _context.AppUsers
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId && u.Employee.OrganizationId == orgId);

        if (user == null)
        {
            return false;
        }

        user.OrgRoleId = orgRoleId;
        await _context.SaveChangesAsync();
        return true;
    }
}
