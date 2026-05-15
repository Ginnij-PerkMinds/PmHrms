using Hangfire;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Repositories;

namespace PmHrmsAPI.PmHrmsBAL.Jobs
{
    public class LeaveAccrualJob
    {
        
        private readonly LeaveDAL _leaveDAL;
        private readonly ILogger<LeaveAccrualJob> _logger;

        public LeaveAccrualJob(LeaveDAL leaveDAL, ILogger<LeaveAccrualJob> logger)
        {
            _leaveDAL = leaveDAL;
            _logger   = logger;
        }

        // Called by Hangfire: runs on 1st of every month
        // Register in Program.cs:
        //   RecurringJob.AddOrUpdate<LeaveAccrualJob>(
        //       "monthly-leave-accrual",
        //       job => job.RunAsync(),
        //       Cron.Monthly(1, 0));        // 1st of month, midnight
        [AutomaticRetry(Attempts = 2)]
        public async Task RunAsync()
        {
            var now   = DateTime.Now;
            var month = now.Month;
            var year  = now.Year;

            _logger.LogInformation("[LeaveAccrual] Starting for {Month}/{Year}", month, year);

            var orgIds = await _leaveDAL.GetAllOrgIds();
              var allEmployees = await _leaveDAL.GetAllActiveEmployees();
              var allRules     = await _leaveDAL.GetAllAllocationRules();

              var empsByOrg  = allEmployees.GroupBy(e => e.OrganizationId)
                                 .ToDictionary(g => g.Key, g => g.ToList());

                var rulesByOrg = allRules.GroupBy(r => r.OrganizationId)
                             .ToDictionary(g => g.Key, g => g.ToList());


            foreach (var orgId in orgIds)
            {
                if (month == 1)
                    {
                        _logger.LogInformation(
                            "[LeaveAccrual] Year-end carry-forward processing for OrgId {OrgId}", orgId);

                        await _leaveDAL.CarryForwardBalances(orgId, fromYear: year - 1, toYear: year);
                    }

                        var orgEmps  = empsByOrg.GetValueOrDefault(orgId)  ?? new();
                        var orgRules = rulesByOrg.GetValueOrDefault(orgId) ?? new();

                        await AccrueForOrg(orgEmps, orgRules, month, year);

            }

            _logger.LogInformation("[LeaveAccrual] Completed for {Month}/{Year}", month, year);
        }

        private async Task AccrueForOrg(
            List<Employee> employees,
            List<LeaveAllocationRule> rules,
            int month, int year)
        {
            var rulesByType = rules.GroupBy(r => r.LeaveTypeId).ToList();
            var today       = DateOnly.FromDateTime(DateTime.Today);

            var balancesToUpsert = new List<LeaveBalance>(); 

            foreach (var emp in employees)
            {
                foreach (var typeRules in rulesByType)
                {
                    var leaveTypeId = typeRules.Key;

                    LeaveAllocationRule? applicableRule = null;

                    if (emp.DesignationId.HasValue)
                    {
                        applicableRule = typeRules.FirstOrDefault(r =>
                            r.LeaveRuleDesignations.Any(d => d.DesignationId == emp.DesignationId.Value));
                    }

                    applicableRule ??= typeRules.FirstOrDefault(r => r.IsDefault);

                    if (applicableRule == null) continue;
                    if (applicableRule.EffectiveFrom > today) continue;
                    if (applicableRule.EffectiveTo.HasValue && applicableRule.EffectiveTo < today) continue;

                    balancesToUpsert.Add(new LeaveBalance  
                    {
                        EmployeeId     = emp.EmployeeId,
                        LeaveTypeId    = leaveTypeId,
                        Month          = month,
                        Year           = year,
                        Balance        = applicableRule.DaysPerMonth,
                        Used           = 0,
                        PreDeducted    = 0,
                        RuleSnapshotId = applicableRule.RuleId,
                        CreatedAt      = DateTime.UtcNow
                    });
                }
            }

            
            if (balancesToUpsert.Any())
            {
                _logger.LogInformation("[LeaveAccrual] Upserting {Count} balance rows", balancesToUpsert.Count);
                await _leaveDAL.BulkUpsertBalances(balancesToUpsert);
            }
        }
    }
}

