using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class EmployeeDAL
    {
        private readonly PmHrmsContext _context;

        public EmployeeDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetTeamMembers(int departmentId, int employeeId)
        {
            return await _context.Employees
                .Include(e => e.Designation)
                .Where(e =>
                    e.DepartmentId == departmentId &&
                    e.EmployeeId != employeeId &&
                    e.IsActive == true)
                .OrderBy(e => e.FirstName)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<(List<Employee>, int totalCount)> GetAllEmployees(int page, int size, string? search)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Policy)
                .Include(e => e.ReportingManager)
                .Include(e => e.SecondaryManager)
                .Include(e => e.AppUsers)
                    .ThenInclude(u => u.OrgRole)
                .Include(e => e.AppUsers)
                    .ThenInclude(u => u.SystemRole)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.EmployeeCode.Contains(search) ||
                    e.OfficialEmail.Contains(search));
            }

            int count = await query.CountAsync();

            var data = await query.OrderByDescending(e => e.EmployeeId)
                                  .Skip((page - 1) * size)
                                  .Take(size) 
                                  .ToListAsync();
           return (data, count);
        }


        public async Task<Employee?> GetEmployee(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Include(e => e.Policy)
                .ThenInclude(p => p.WeekOffs)
                .Include(e => e.ReportingManager)
                .Include(e => e.SecondaryManager)

                .Include(e => e.AppUsers)
                    .ThenInclude(u => u.OrgRole)
                .Include(e => e.AppUsers)
                    .ThenInclude(u => u.SystemRole)
                .Include(e => e.EmployeeDetail)
                    .ThenInclude(ed => ed.CurrentState)
                .Include(e => e.EmployeeDetail)
                    .ThenInclude(ed => ed.CurrentCountry)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<OfficeLocation?> GetOfficeLocation(int locationId)
        {
            return await _context.OfficeLocations
                .Where(x => x.LocationId == locationId)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> EmailExists(string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.OfficialEmail == email);
        }

        public async Task<bool> EmployeeCodeExists(string code, int orgId)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == code && e.OrganizationId == orgId);
        }

        public async Task<bool> PhoneExists(string phone)
        {
            return await _context.Employees
                .AnyAsync(e => e.PhoneNumber == phone);
        }


        public async Task<bool> EmailExistsForOther(int id, string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeId != id && e.OfficialEmail == email);
        }

 
        public async Task<Employee> AddEmployee(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

    
        public async Task<Employee?> UpdateEmployee(Employee employee)
        {
            var existing = await _context.Employees.FindAsync(employee.EmployeeId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(employee);
            await _context.SaveChangesAsync();

            return existing;
        }


        public async Task<List<OrgRole>> GetOrgRoles(int orgId)
        {
            return await _context.OrgRoles
                .Where(r => r.OrgId == orgId)
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OrgRole?> GetOrgRoleById(int orgRoleId)
        {
            return await _context.OrgRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrgRoleId == orgRoleId);
        }



        public async Task<List<SystemRole>> GetSystemRoles()
        {
            return await _context.SystemRoles
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }

   
        public async Task<bool> DeleteEmployee(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return false;

            emp.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        // --- Employee Bank Accounts ---
        public async Task<List<EmployeeBankAccount>> GetBankAccountsByEmployee(int employeeId, int orgId)
        {
            return await _context.EmployeeBankAccounts
                .Where(b => b.EmployeeId == employeeId && b.OrganizationId == orgId && b.IsActive)
                .OrderByDescending(b => b.IsPrimary)
                .ToListAsync();
        }

        public async Task<EmployeeBankAccount?> GetBankAccountById(int bankAccountId)
        {
            return await _context.EmployeeBankAccounts
                .FirstOrDefaultAsync(b => b.BankAccountId == bankAccountId && b.IsActive);
        }

        public async Task<EmployeeBankAccount> AddBankAccount(EmployeeBankAccount entity)
        {
            await _context.EmployeeBankAccounts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<EmployeeBankAccount?> UpdateBankAccount(EmployeeBankAccount entity)
        {
            var existing = await _context.EmployeeBankAccounts.FindAsync(entity.BankAccountId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteBankAccount(int bankAccountId)
        {
            var existing = await _context.EmployeeBankAccounts.FindAsync(bankAccountId);
            if (existing == null) return false;
            existing.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SetPrimaryBankAccount(int bankAccountId, int employeeId, int orgId)
        {
            var target = await _context.EmployeeBankAccounts
                .FirstOrDefaultAsync(b => b.BankAccountId == bankAccountId && b.EmployeeId == employeeId && b.OrganizationId == orgId && b.IsActive);
            if (target == null) return;

            var others = await _context.EmployeeBankAccounts
                .Where(b => b.EmployeeId == employeeId && b.OrganizationId == orgId && b.IsActive && b.BankAccountId != bankAccountId)
                .ToListAsync();

            foreach (var o in others) o.IsPrimary = false;
            target.IsPrimary = true;

            await _context.SaveChangesAsync();
        }

      

    }

}

