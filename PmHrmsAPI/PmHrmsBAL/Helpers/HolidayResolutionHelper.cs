
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Helpers
{
    public static class HolidayResolutionHelper
    {
        public static (HolidayGroup? group, HolidayResolutionSource source) ResolveGroup(
            Employee employee, List<HolidayGroup> allGroups)
        {
            if (employee.HolidayGroupId.HasValue)
            {
                var direct = allGroups.FirstOrDefault(g => g.Id == employee.HolidayGroupId.Value);
                if (direct is not null)
                    return (direct, HolidayResolutionSource.DirectAssignment);
            }

            var ruleMatch = allGroups
                .Where(g => !g.IsDefault && g.EligibilityRules.Count > 0)
                .FirstOrDefault(g => g.EligibilityRules.Any(r => MatchesRule(employee, r)));

            if (ruleMatch is not null)
                return (ruleMatch, HolidayResolutionSource.EligibilityRule);

            var defaultGroup = allGroups.FirstOrDefault(g => g.IsDefault);
            if (defaultGroup is not null)
                return (defaultGroup, HolidayResolutionSource.DefaultGroup);

            return (null, HolidayResolutionSource.None);
        }

        public static bool MatchesRule(Employee employee, HolidayGroupEligibility rule)
        {
            var locMatch  = !rule.OfficeLocationId.HasValue || rule.OfficeLocationId == employee.AssignedOfficeId;
            var deptMatch = !rule.DepartmentId.HasValue     || rule.DepartmentId     == employee.DepartmentId;
            return locMatch && deptMatch;
        }
    }
}