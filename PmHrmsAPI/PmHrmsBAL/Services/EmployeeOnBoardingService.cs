using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    
    public class EmployeeOnboardingService : IEmployeeOnboardingService
    {
        private readonly PmHrmsContext _db;
        private readonly ILogger<EmployeeOnboardingService> _logger;

        public EmployeeOnboardingService(PmHrmsContext db, ILogger<EmployeeOnboardingService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task ApplyDefaultsAsync(List<Employee> employees, int orgId)
        {
            if (!employees.Any()) return;

            var now   = DateTime.UtcNow;
            var month = now.Month;
            var year  = now.Year;

            var defaultOffice = await _db.OfficeLocations
                .FirstOrDefaultAsync(o => o.OrganizationId == orgId && o.IsDefault == true);

            if (defaultOffice != null)
                _logger.LogDebug("[Onboarding] Default office found: {Id} for OrgId {OrgId}",
                    defaultOffice.LocationId, orgId);
            else
                _logger.LogDebug("[Onboarding] No default office for OrgId {OrgId}", orgId);

            
            var defaultPolicy = await _db.WorkPolicies
                .FirstOrDefaultAsync(p => p.OrganizationId == orgId && p.IsDefault == true && p.IsActive == true);

            if (defaultPolicy != null)
                _logger.LogDebug("[Onboarding] Default policy found: {Id} for OrgId {OrgId}",
                    defaultPolicy.PolicyId, orgId);
            else
                _logger.LogDebug("[Onboarding] No default work policy for OrgId {OrgId}", orgId);

            
            var allRules = await _db.LeaveAllocationRules
                .Include(r => r.LeaveRuleDesignations)
                .Include(r => r.LeaveType)
                .Where(r => r.OrganizationId == orgId
                         && r.LeaveType.IsActive
                         && r.EffectiveFrom <= DateOnly.FromDateTime(now)
                         && (r.EffectiveTo == null || r.EffectiveTo >= DateOnly.FromDateTime(now)))
                .ToListAsync();

            _logger.LogDebug("[Onboarding] {Count} active rules loaded for OrgId {OrgId}",
                allRules.Count, orgId);

           
            foreach (var emp in employees)
            {
                // Office
                if (emp.AssignedOfficeId == null && defaultOffice != null)
                    emp.AssignedOfficeId = defaultOffice.LocationId;

                // Policy
                if (emp.PolicyId == null && defaultPolicy != null)
                    emp.PolicyId = defaultPolicy.PolicyId;

                // Leave balance — one entry per leave type
                await InitialiseLeaveBalanceAsync(emp, allRules, month, year);
            }
        }

        
        public async Task ApplyDefaultsAsync(Employee employee, int orgId) =>
            await ApplyDefaultsAsync(new List<Employee> { employee }, orgId);

                private async Task InitialiseLeaveBalanceAsync(
            Employee emp,
            List<LeaveAllocationRule> allRules,
            int month, int year)
        {
            // Group rules by leave type
            var byType = allRules.GroupBy(r => r.LeaveTypeId);

            foreach (var group in byType)
            {
                var leaveTypeId = group.Key;

                
                LeaveAllocationRule? rule = null;

                if (emp.DesignationId.HasValue)
                {
                    rule = group.FirstOrDefault(r =>
                        r.LeaveRuleDesignations.Any(d => d.DesignationId == emp.DesignationId.Value));
                }

                rule ??= group.FirstOrDefault(r => r.IsDefault);

                if (rule == null)
                {
                    _logger.LogDebug(
                        "[Onboarding] No applicable rule for EmpId {EmpId} LeaveType {LtId} — skipping",
                        emp.EmployeeId, leaveTypeId);
                    continue;
                }

                
                var alreadyExists = await _db.LeaveBalance.AnyAsync(b =>
                    b.EmployeeId  == emp.EmployeeId &&
                    b.LeaveTypeId == leaveTypeId    &&
                    b.Month       == month          &&
                    b.Year        == year);

                if (alreadyExists)
                {
                    _logger.LogDebug(
                        "[Onboarding] Balance already exists for EmpId {EmpId} TypeId {LtId} {M}/{Y} — skipping",
                        emp.EmployeeId, leaveTypeId, month, year);
                    continue;
                }

                _db.LeaveBalance.Add(new LeaveBalance
                {
                    EmployeeId      = emp.EmployeeId,
                    LeaveTypeId     = leaveTypeId,
                    Month           = month,
                    Year            = year,
                    Balance         = rule.DaysPerMonth,
                    Used            = 0,
                    PreDeducted     = 0,
                    RuleSnapshotId  = rule.RuleId,
                    CreatedAt       = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "[Onboarding] Balance initialised | EmpId {EmpId} | LeaveType {LtId} | Rule '{Rule}' | Days {Days} | {M}/{Y}",
                    emp.EmployeeId, leaveTypeId, rule.RuleName, rule.DaysPerMonth, month, year);
            }
        }
    }
}