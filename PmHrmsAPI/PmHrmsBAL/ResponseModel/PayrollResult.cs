using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public sealed class PayrollResult
    {
        public decimal TotalEarnings { get; init; }
        public decimal TotalDeductions { get; init; }
        public decimal NetPayable { get; init; }
        public decimal LopDeduction { get; init; }
        public decimal RoundingAdjustment { get; init; }
        public List<ComponentResult> Components { get; init; } = new();
    }
}
