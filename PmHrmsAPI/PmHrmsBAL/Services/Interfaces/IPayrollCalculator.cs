using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IPayrollCalculator
    {
        PayrollResult Calculate(PayrollContext context);
    }
}
