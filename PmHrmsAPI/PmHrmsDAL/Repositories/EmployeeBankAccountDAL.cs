using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class EmployeeBankAccountDAL
    {
        private readonly PmHrmsContext _context;

        public EmployeeBankAccountDAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<EmployeeBankAccount> AddBankAccountAsync(EmployeeBankAccount bankAccount)
        {
            if (bankAccount == null)
                throw new ArgumentNullException(nameof(bankAccount));

            await _context.EmployeeBankAccounts.AddAsync(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount;
        }

        public async Task<EmployeeBankAccount?> UpdateBankAccountAsync(EmployeeBankAccount bankAccount)
        {
            if (bankAccount == null)
                throw new ArgumentNullException(nameof(bankAccount));

            var existingAccount = await _context.EmployeeBankAccounts.FirstOrDefaultAsync(x => x.BankAccountId == bankAccount.BankAccountId);

            if (existingAccount == null)
                return null;

            _context.Entry(existingAccount).CurrentValues.SetValues(bankAccount);
            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task DeleteBankAccountAsync(int bankAccountId)
        {
            var bankAccount = await _context.EmployeeBankAccounts.FirstOrDefaultAsync(x => x.BankAccountId == bankAccountId);
            if (bankAccount != null)
            {
                bankAccount.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EmployeeBankAccount?> GetBankAccountByIdAsync(int bankAccountId)
        {
            return await _context.EmployeeBankAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.BankAccountId == bankAccountId);
        }

        public async Task<IEnumerable<EmployeeBankAccount>> GetBankAccountsByEmployeeIdAsync(int employeeId, bool includeInactive = false)
        {
            var query = _context.EmployeeBankAccounts
            .IgnoreQueryFilters()
            .AsNoTracking().Where(x => x.EmployeeId == employeeId);
            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }
            return await query.ToListAsync();
        }

        public async Task<EmployeeBankAccount?> GetPrimaryBankAccountByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeBankAccounts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsPrimary && x.IsActive);
        }

        public async Task SetAllEmployeeAccountsNonPrimaryAsync(int employeeId)
        {
            var primaryAccounts = await _context.EmployeeBankAccounts.Where(x => x.EmployeeId == employeeId && x.IsPrimary).ToListAsync();
            foreach (var account in primaryAccounts)
            {
                account.IsPrimary = false;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> BankAccountExistsAsync(int bankAccountId)
        {
            return await _context.EmployeeBankAccounts.AnyAsync(x => x.BankAccountId == bankAccountId);
        }
    }
}
