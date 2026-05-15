using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IEmployeeBankAccountBAL
    {
        Task<BankAccountResponseModel> AddBankAccountAsync(CreateBankAccountModel createDto);
        Task<BankAccountResponseModel?> UpdateBankAccountAsync(UpdateBankAccountModel updateDto);
        Task DeleteBankAccountAsync(int bankAccountId);
        Task<BankAccountResponseModel?> GetBankAccountByIdAsync(int bankAccountId);
        Task<IEnumerable<BankAccountResponseModel>> GetBankAccountsByEmployeeIdAsync(int employeeId);
    }
}
