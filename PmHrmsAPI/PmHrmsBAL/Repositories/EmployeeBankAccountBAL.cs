using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;

using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class EmployeeBankAccountBAL : IEmployeeBankAccountBAL
    {
        private readonly EmployeeBankAccountDAL _bankAccountDAL;

        public EmployeeBankAccountBAL(EmployeeBankAccountDAL bankAccountDAL)
        {
            _bankAccountDAL = bankAccountDAL;
        }

        public async Task<BankAccountResponseModel> AddBankAccountAsync(CreateBankAccountModel createDto)
        {
            var bankAccount = new EmployeeBankAccount
            {
                EmployeeId = createDto.EmployeeId,
                OrganizationId = createDto.OrganizationId,
                AccountHolderName = createDto.AccountHolderName,
                AccountNumber = createDto.AccountNumber,
                IFSCCode = createDto.IFSCCode,
                BankName = createDto.BankName,
                BranchName = createDto.BranchName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var existingAccounts = await _bankAccountDAL.GetBankAccountsByEmployeeIdAsync(createDto.EmployeeId);

            if (!existingAccounts.Any())
            {
                // If no accounts exist, the first one becomes primary
                bankAccount.IsPrimary = true;
            }
            else if (createDto.IsPrimary)
            {
                // If new account is marked primary, set all others to non-primary
                await _bankAccountDAL.SetAllEmployeeAccountsNonPrimaryAsync(createDto.EmployeeId);
                bankAccount.IsPrimary = true;
            }
            else
            {
                bankAccount.IsPrimary = false;
            }

            var addedAccount = await _bankAccountDAL.AddBankAccountAsync(bankAccount);
            return MapToResponseDto(addedAccount);
        }

        public async Task<BankAccountResponseModel?> UpdateBankAccountAsync(UpdateBankAccountModel updateDto)
        {
            var existingAccount = await _bankAccountDAL.GetBankAccountByIdAsync(updateDto.BankAccountId);
            if (existingAccount == null)
            {
                return null; // Account not found
            }

            // Ensure the EmployeeId and OrganizationId are not changed during update
            if (existingAccount.EmployeeId != updateDto.EmployeeId || existingAccount.OrganizationId != updateDto.OrganizationId)
            {
                //throw new ArgumentException("EmployeeId and OrganizationId cannot be changed during bank account update.");
                throw new ArgumentException(PmHrmsConstants.EmployeeBankAccountMessages.InvalidUpdate);
            }

            existingAccount.AccountHolderName = updateDto.AccountHolderName;
            existingAccount.AccountNumber = updateDto.AccountNumber;
            existingAccount.IFSCCode = updateDto.IFSCCode;
            existingAccount.BankName = updateDto.BankName;
            existingAccount.BranchName = updateDto.BranchName;
            existingAccount.IsActive = updateDto.IsActive;

            if (updateDto.IsPrimary && !existingAccount.IsPrimary)
            {
                // If this account is being set as primary, set all others to non-primary
                await _bankAccountDAL.SetAllEmployeeAccountsNonPrimaryAsync(updateDto.EmployeeId);
                existingAccount.IsPrimary = true;
            }
            else if (!updateDto.IsPrimary && existingAccount.IsPrimary)
            {
                // If this account was primary and is being set to non-primary, ensure another active account becomes primary
                existingAccount.IsPrimary = false;
                var updatedAccount = await _bankAccountDAL.UpdateBankAccountAsync(existingAccount);

                var activeAccounts = (await _bankAccountDAL.GetBankAccountsByEmployeeIdAsync(updateDto.EmployeeId))
                                     .Where(x => x.IsActive && x.BankAccountId != updateDto.BankAccountId).ToList();

                if (activeAccounts.Any() && !activeAccounts.Any(x => x.IsPrimary))
                {
                    // Assign the first active account as primary if no other primary exists
                    var newPrimary = activeAccounts.First();
                    newPrimary.IsPrimary = true;
                    await _bankAccountDAL.UpdateBankAccountAsync(newPrimary);
                }
                return MapToResponseDto(updatedAccount);
            }
            else
            {
                existingAccount.IsPrimary = updateDto.IsPrimary;
            }

            var updatedAccountResult = await _bankAccountDAL.UpdateBankAccountAsync(existingAccount);
            return updatedAccountResult != null ? MapToResponseDto(updatedAccountResult) : null;
        }

        public async Task DeleteBankAccountAsync(int bankAccountId)
        {
            var bankAccountToDelete = await _bankAccountDAL.GetBankAccountByIdAsync(bankAccountId);
            if (bankAccountToDelete == null)
            {
                return; // Account not found, nothing to delete
            }

            var employeeId = bankAccountToDelete.EmployeeId;
            var wasPrimary = bankAccountToDelete.IsPrimary;

            await _bankAccountDAL.DeleteBankAccountAsync(bankAccountToDelete.BankAccountId); // Soft delete

            if (wasPrimary)
            {
                // If the deleted account was primary, assign another active account as primary
                var activeAccounts = (await _bankAccountDAL.GetBankAccountsByEmployeeIdAsync(employeeId))
                                     .Where(x => x.IsActive).ToList();

                if (activeAccounts.Any() && !activeAccounts.Any(x => x.IsPrimary))
                {
                    // Assign the first active account as primary if no other primary exists
                    var newPrimary = activeAccounts.First();
                    newPrimary.IsPrimary = true;
                    await _bankAccountDAL.UpdateBankAccountAsync(newPrimary);
                }
            }
        }

        public async Task<BankAccountResponseModel?> GetBankAccountByIdAsync(int bankAccountId)
        {
            var bankAccount = await _bankAccountDAL.GetBankAccountByIdAsync(bankAccountId);
            return bankAccount != null ? MapToResponseDto(bankAccount) : null;
        }

        public async Task<IEnumerable<BankAccountResponseModel>> GetBankAccountsByEmployeeIdAsync(int employeeId)
        {
            var bankAccounts = await _bankAccountDAL.GetBankAccountsByEmployeeIdAsync(employeeId);
            return bankAccounts.Select(MapToResponseDto);
        }

        private BankAccountResponseModel MapToResponseDto(EmployeeBankAccount bankAccount)
        {
            return new BankAccountResponseModel
            {
                BankAccountId = bankAccount.BankAccountId,
                EmployeeId = bankAccount.EmployeeId,
                OrganizationId = bankAccount.OrganizationId,
                AccountHolderName = bankAccount.AccountHolderName,
                AccountNumber = bankAccount.AccountNumber,
                IFSCCode = bankAccount.IFSCCode,
                BankName = bankAccount.BankName,
                BranchName = bankAccount.BranchName,
                IsPrimary = bankAccount.IsPrimary,
                IsActive = bankAccount.IsActive,
                CreatedAt = bankAccount.CreatedAt
            };
        }
    }
}
