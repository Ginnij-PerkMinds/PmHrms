using System;
using System.Collections.Generic;
using System.Linq;
using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public sealed class PayrollCalculator : IPayrollCalculator
    {
        private const string EarningType = "Earning";
        private const string DeductionType = "Deduction";

        public PayrollResult Calculate(PayrollContext context)
        {
            if (context.TotalWorkingDays == 0)
            {
                throw new ArgumentException("Total working days must be greater than zero.", nameof(context));
            }

            if (context.GrossSalary < 0)
            {
                throw new ArgumentException("Gross salary cannot be negative.", nameof(context));
            }

            var components = new List<ComponentResult>();
            var grossSalary = RoundMoney(context.GrossSalary);


            var structureEarnings = context.SalaryComponents
        .Where(c => c.IsEarning)
        .ToList();

            if (structureEarnings.Count > 0)
            {
                foreach (var earning in structureEarnings)
                {
                    components.Add(new ComponentResult
                    {
                        Name   = earning.ComponentName,
                        Amount = RoundMoney(earning.Amount),
                        Type   = EarningType
                    });
                }
            }
            else
            {
                // Fallback: no itemized earnings → single gross salary line
                components.Add(new ComponentResult
                {
                    Name   = "Gross Salary",
                    Amount = grossSalary,
                    Type   = EarningType
                });
            }


            var unpaidLeaveDays = ClampUnpaidLeaveDays(context.UnpaidLeaveDays, context.TotalWorkingDays);
    if (unpaidLeaveDays > 0)
    {
        var perDaySalary = RoundMoney(grossSalary / context.TotalWorkingDays);
        var lopDeduction = RoundMoney(perDaySalary * unpaidLeaveDays);
        if (lopDeduction > 0)
        {
            components.Add(new ComponentResult
            {
                Name   = "LOP Deduction",
                Amount = lopDeduction,
                Type   = DeductionType
            });
        }
    }

            var structureDeductions = context.SalaryComponents
                .Where(c => !c.IsEarning)
                .ToList();

            foreach (var deduction in structureDeductions)
            {
                if (deduction.Amount <= 0) continue;

                components.Add(new ComponentResult
                {
                    Name   = deduction.ComponentName,
                    Amount = RoundMoney(deduction.Amount),
                    Type   = DeductionType
                });
            }



            AddPercentageDeduction(
                components,
                "PF Employee Contribution",
                context.IsPfApplicable,
                CalculatePfBase(grossSalary, context.PfWageLimit),
                context.PfEmployeePercentage);

            AddPercentageDeduction(
                components,
                "ESI Employee Contribution",
                context.IsEsiApplicable,
                grossSalary,
                context.EsiEmployeePercentage);

            AddPercentageDeduction(
                components,
                "TDS Deduction",
                context.IsTdsApplicable,
                grossSalary,
                context.TdsPercentage);

              var totalEarnings   = components.Where(c => c.Type == EarningType).Sum(c => c.Amount);
             var totalDeductions = components.Where(c => c.Type == DeductionType).Sum(c => c.Amount);

              var rawNetPayable    = totalEarnings - totalDeductions;
             var netPayable       = ApplyRounding(rawNetPayable, context.RoundingRule);
            var roundingAdj      = RoundMoney(netPayable - rawNetPayable);

             return new PayrollResult
    {
        TotalEarnings      = RoundMoney(totalEarnings),
        TotalDeductions    = RoundMoney(totalDeductions),
        NetPayable         = RoundMoney(netPayable),
        LopDeduction       = components.FirstOrDefault(c => c.Name == "LOP Deduction")?.Amount ?? 0m,
        RoundingAdjustment = roundingAdj,
        Components         = components
    };
}

        private static void AddPercentageDeduction(
            ICollection<ComponentResult> components,
            string name,
            bool isApplicable,
            decimal baseAmount,
            decimal? percentage)
        {
            var percentageValue = percentage.GetValueOrDefault();
            if (!isApplicable || percentageValue <= 0 || baseAmount <= 0)
            {
                return;
            }

            var amount = RoundMoney(baseAmount * percentageValue / 100m);
            if (amount <= 0)
            {
                return;
            }

            components.Add(new ComponentResult
            {
                Name = name,
                Amount = amount,
                Type = DeductionType
            });
        }

        private static decimal CalculatePfBase(decimal grossSalary, decimal? wageLimit)
        {
            var wageLimitValue = wageLimit.GetValueOrDefault();
            if (wageLimitValue <= 0)
            {
                return grossSalary;
            }

            return Math.Min(grossSalary, wageLimitValue);
        }

        private static decimal ClampUnpaidLeaveDays(decimal unpaidLeaveDays, byte totalWorkingDays)
        {
            if (unpaidLeaveDays <= 0)
            {
                return 0;
            }

            return Math.Min(unpaidLeaveDays, totalWorkingDays);
        }

        private static decimal ApplyRounding(decimal amount, string? roundingRule)
        {
            return roundingRule switch
            {
                "Floor" => Math.Floor(amount),
                "Ceil" => Math.Ceiling(amount),
                _ => Math.Round(amount, 0, MidpointRounding.AwayFromZero)
            };
        }

        private static decimal RoundMoney(decimal amount)
        {
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
