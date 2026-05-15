using Hangfire;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Helpers;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class HolidayBAL : IHolidayBAL
    {
        private readonly HolidayDAL _holidayDAL;
        private readonly IPermissionService _permissionService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<HolidayBAL> _logger;
        private readonly EmployeeDAL _employeeDAL;

        public HolidayBAL(
            HolidayDAL holidayDAL,
             EmployeeDAL employeeDAL,
            IPermissionService permissionService,
            IBackgroundJobClient backgroundJobClient,
            ILogger<HolidayBAL> logger)
        {
            _holidayDAL = holidayDAL;
            _employeeDAL = employeeDAL;
            _permissionService = permissionService;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

      
        public async Task<object> GetMasterHolidays(int orgId, int year)
        {
            _permissionService.Ensure(PermissionKeys.HOLIDAY_VIEW);

            var holidays = await _holidayDAL.GetMasterHolidaysForOrg(orgId, year);

            return holidays.Select(h => new
            {
                h.Id,
                h.HolidayName,
                h.HolidayDate,
                h.IsCustom,
                h.IsRecurring
            }).ToList();
        }

        
        public async Task<object> GetHolidayGroups(int orgId, int year)
        {
            _permissionService.Ensure(PermissionKeys.HOLIDAY_VIEW);

            var groups = await _holidayDAL.GetHolidayGroupsByOrgAndYear(orgId, year);

            return groups
                .OrderByDescending(g => g.IsDefault)
                .ThenBy(g => g.GroupName)
                .Select(g => new
                {
                    g.Id,
                    g.GroupName,
                    g.Year,
                    g.IsActive,
                    g.IsDefault,
                    g.Description,
                    Holidays = g.GroupHolidays
                        .OrderBy(gh => gh.SystemHoliday.HolidayDate)
                        .Select(gh => new
                        {
                            gh.SystemHoliday.Id,
                            gh.SystemHoliday.HolidayName,
                            gh.SystemHoliday.HolidayDate,
                            gh.IsOptional
                        }).ToList(),
                    Rules = g.EligibilityRules
                        .OrderBy(r => r.OfficeLocationId)
                        .ThenBy(r => r.DepartmentId)
                        .Select(r => new
                        {
                            r.OfficeLocationId,
                            r.DepartmentId
                        }).ToList()
                }).ToList();
        }

        public async Task<object> GetEmployeeHolidays(int employeeId, int year)
        {
            var employee = await _employeeDAL.GetEmployee(employeeId);

            if (employee == null)
                //throw new ArgumentException("Employee not found");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.EmployeeNotFound);



            var allGroups = await _holidayDAL.GetActiveGroupsWithDetailsAsync(
              employee.OrganizationId, year);

              var (resolvedGroup, source) = HolidayResolutionHelper.ResolveGroup(employee, allGroups);

            _logger.LogDebug(
                    "Holiday resolution for Employee {EmployeeId} | Year {Year} | Source: {Source} | GroupId: {GroupId}",
                    employeeId,
                    year,
                    source,
                    resolvedGroup?.Id);

                var holidays = resolvedGroup is not null
                    ? ProjectHolidays(resolvedGroup, year)
                    : new List<object>();
         
            var weekOffs = employee.Policy?.WeekOffs
                ?.Select(w => new
                {
                    day = w.DayOfWeek.ToString(),   
                    isHalfDay = w.IsHalfDay,        
                    //type = "WEEKOFF"
                    type = PmHrmsConstants.HolidayMessages.WeekOff.ToString()
                })
                .ToList();

            return new
            {
                holidays,
                weekOffs,
             meta = new
                {
                    resolvedGroupId   = resolvedGroup?.Id,
                    resolvedGroupName = resolvedGroup?.GroupName,
                    resolutionSource  = source.ToString()  
                }
            };
        }




        private static List<object> ProjectHolidays(HolidayGroup group, int year)
        {
            return group.GroupHolidays
                .Where(gh => gh.SystemHoliday.HolidayDate.Year == year)  // year filter
                .OrderBy(gh => gh.SystemHoliday.HolidayDate)
                .Select(gh => (object)new
                {
                    date       = gh.SystemHoliday.HolidayDate,
                    name       = gh.SystemHoliday.HolidayName,
                    //type       = "HOLIDAY",
                    type = PmHrmsConstants.HolidayMessages.Holiday.ToString(),
                    isOptional = gh.IsOptional
                })
                .ToList();
        }


        public async Task<object> SaveHolidayGroup(int orgId, SaveHolidayGroupRequest request)
        {
            if (request.GroupId <= 0)
                _permissionService.Ensure(PermissionKeys.HOLIDAY_CREATE);
            else
                _permissionService.Ensure(PermissionKeys.HOLIDAY_EDIT);

            if (request.Year <= 0)
                //throw new ArgumentException("Year is required.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.YearRequired);

            var groupName = request.GroupName?.Trim();
            if (string.IsNullOrWhiteSpace(groupName))
                //throw new ArgumentException("Group name is required.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.GroupNameRequired);

            var selectedHolidayIds = request.SystemHolidayIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (selectedHolidayIds.Count == 0)
                //throw new ArgumentException("Select at least one holiday.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.SelectHolidayRequired);

            var availableHolidayIds = (await _holidayDAL.GetMasterHolidaysForOrg(orgId, request.Year))
                .Select(h => h.Id)
                .ToHashSet();

            var invalidHolidayIds = selectedHolidayIds
                .Where(id => !availableHolidayIds.Contains(id))
                .ToList();

            if (invalidHolidayIds.Count > 0)
                //throw new ArgumentException("One or more selected holidays are invalid for this organization and year.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.InvalidHolidaySelection);

            var normalizedRules = NormalizeEligibilityRules(request.EligibilityRules);

            HolidayGroup group;

            if (request.GroupId <= 0)
            {
                if (request.IsDefault)
                {
                    await _holidayDAL.ClearDefaultHolidayGroups(orgId, request.Year);
                }

                group = new HolidayGroup
                {
                    OrganizationId = orgId,
                    GroupName = groupName,
                    Year = request.Year,
                    Description = NormalizeDescription(request.Description),
                    IsActive = true,
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.UtcNow
                };

                await _holidayDAL.AddHolidayGroup(group);
            }
            else
            {
                group = await _holidayDAL.GetHolidayGroupById(request.GroupId, orgId)
                    //?? throw new ArgumentException("Holiday group not found.");
                    ?? throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayGroupNotFound);

                group.GroupName = groupName;
                group.Description = NormalizeDescription(request.Description);
                group.Year = request.Year;
                group.IsDefault = request.IsDefault;

                if (request.IsDefault)
                {
                    await _holidayDAL.ClearDefaultHolidayGroups(orgId, request.Year, group.Id);
                }

                _holidayDAL.RemoveGroupMappingsAndRules(group);
                group.GroupHolidays.Clear();
                group.EligibilityRules.Clear();
            }

            foreach (var holidayId in selectedHolidayIds)
            {
                group.GroupHolidays.Add(new HolidayGroupMapping
                {
                    SystemHolidayId = holidayId
                });
            }

            foreach (var rule in normalizedRules)
            {
                group.EligibilityRules.Add(new HolidayGroupEligibility
                {
                    OfficeLocationId = rule.OfficeLocationId,
                    DepartmentId = rule.DepartmentId
                });
            }

            await _holidayDAL.SaveChangesAsync();

            var assignmentJobId = _backgroundJobClient.Enqueue<HolidayBAL>(
                bal => bal.AssignHolidayGroupToEligibleEmployeesAsync(orgId, group.Id));

            return new
            {
                GroupId = group.Id,
                AssignmentJobId = assignmentJobId,
                //Message = "Saved successfully. Employee assignment queued."
                Message = PmHrmsConstants.HolidayMessages.SavedSuccessfully
            };
        }

        public async Task DeleteHolidayGroup(int orgId, int groupId)
        {
            _permissionService.Ensure(PermissionKeys.HOLIDAY_DELETE);

            var group = await _holidayDAL.GetHolidayGroupById(groupId, orgId)
                //?? throw new ArgumentException("Group not found.");
                ?? throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayGroupNotFound);

            await _holidayDAL.ClearHolidayGroupAssignments(orgId, groupId);
            _holidayDAL.RemoveGroupMappingsAndRules(group);
            _holidayDAL.RemoveHolidayGroup(group);
            await _holidayDAL.SaveChangesAsync();
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task AssignHolidayGroupToEligibleEmployeesAsync(int orgId, int groupId)
        {
            var group = await _holidayDAL.GetHolidayGroupById(groupId, orgId);
            if (group == null)
            {
                _logger.LogWarning(               
                    "Skipping holiday group assignment because group {GroupId} was not found for org {OrgId}.",
                    groupId,  
                    orgId);
                return;
            }

            var rules = NormalizeEligibilityRules(group.EligibilityRules
                .Select(r => new GroupEligibilityRule
                {
                    OfficeLocationId = r.OfficeLocationId,
                    DepartmentId = r.DepartmentId
                })
                .ToList());

            var employees = await _holidayDAL.GetEmployeesForHolidayGroupSync(orgId);
            var eligibleEmployeeIds = employees
                .Where(e => e.IsActive && MatchesAnyRule(e, rules))
                .Select(e => e.EmployeeId)
                .ToHashSet();

            var assignedCount = 0;
            var unassignedCount = 0;

            foreach (var employee in employees)
            {
                var hasOtherActiveGroupForYear =
                    employee.HolidayGroupId.HasValue &&
                    employee.HolidayGroupId != groupId &&
                    employee.HolidayGroup?.IsActive == true &&
                    employee.HolidayGroup.Year == group.Year;

                if (eligibleEmployeeIds.Contains(employee.EmployeeId))
                {
                    if (group.IsDefault && hasOtherActiveGroupForYear)
                    {
                        continue;
                    }

                    if (employee.HolidayGroupId != groupId)
                    {
                        employee.HolidayGroupId = groupId;
                        assignedCount++;
                    }

                    continue;
                }

                if (employee.HolidayGroupId == groupId)
                {
                    employee.HolidayGroupId = null;
                    unassignedCount++;
                }
            }

            if (assignedCount > 0 || unassignedCount > 0)
            {
                await _holidayDAL.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Holiday group sync completed for org {OrgId}, group {GroupId}. Assigned: {AssignedCount}, Unassigned: {UnassignedCount}.",
                orgId,
                groupId,
                assignedCount,
                unassignedCount);
        }

        // â”€â”€ 3. CUSTOM HOLIDAYS â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<object> AddCustomHoliday(int orgId, AddCustomHolidayRequest request)
        {
            _permissionService.Ensure(PermissionKeys.HOLIDAY_CREATE);

            var normalizedName = request.HolidayName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                //throw new ArgumentException("Holiday name is required.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayNameRequired);
            if (request.HolidayDate == default)
                //throw new ArgumentException("Holiday date is required.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayDateRequired);
            if (request.Year <= 0)
                //throw new ArgumentException("Year is required.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.YearRequired);
            if (request.HolidayDate.Year != request.Year)
                //throw new ArgumentException("Holiday date must fall within the selected year.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayDateYearMismatch);

            var existingHolidays = await _holidayDAL.GetMasterHolidaysForOrg(orgId, request.Year);
            if (existingHolidays.Any(h =>
                h.HolidayDate == request.HolidayDate &&
                h.HolidayName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                //throw new ArgumentException("A holiday with this name already exists on that date.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayAlreadyExists);
            }

            var customHoliday = new SystemHoliday
            {
                HolidayName = normalizedName,
                HolidayDate = request.HolidayDate,
                Year = request.Year,
                //CountryCode = "IN",
                CountryCode = PmHrmsConstants.HolidayMessages.India,
                IsRecurring = request.IsRecurring,
                IsCustom = true,
                CreatedByOrgId = orgId,
                CreatedAt = DateTime.UtcNow
            };

            await _holidayDAL.AddSystemHoliday(customHoliday);
            await _holidayDAL.SaveChangesAsync();

            return new
            {
                customHoliday.Id,
                customHoliday.HolidayName,
                customHoliday.HolidayDate,
                customHoliday.IsCustom,
                customHoliday.IsRecurring
            };
        }

        public async Task DeleteCustomHoliday(int orgId, int systemHolidayId)
        {
            _permissionService.Ensure(PermissionKeys.HOLIDAY_DELETE);

            var systemHoliday = await _holidayDAL.GetSystemHolidayById(systemHolidayId)
                //?? throw new ArgumentException("Holiday not found.");
                ?? throw new ArgumentException(PmHrmsConstants.HolidayMessages.HolidayNotFound);

            if (!systemHoliday.IsCustom || systemHoliday.CreatedByOrgId != orgId)
                //throw new ArgumentException("You can only delete custom holidays created by your organization.");
                throw new ArgumentException(PmHrmsConstants.HolidayMessages.DeleteCustomHoliday);

            var dependentMappings = await _holidayDAL.GetAssignmentsBySystemHolidayAndOrg(orgId, systemHolidayId);
            if (dependentMappings.Any())
            {
                _holidayDAL.RemoveGroupMappings(dependentMappings);
            }

            _holidayDAL.RemoveSystemHolidays(new List<SystemHoliday> { systemHoliday });
            await _holidayDAL.SaveChangesAsync();
        }

        private static bool MatchesAnyRule(Employee employee, IEnumerable<GroupEligibilityRule> rules)
        {
            return rules.Any(rule =>
                (!rule.OfficeLocationId.HasValue || employee.AssignedOfficeId == rule.OfficeLocationId.Value) &&
                (!rule.DepartmentId.HasValue || employee.DepartmentId == rule.DepartmentId.Value));
        }

        private static List<GroupEligibilityRule> NormalizeEligibilityRules(
            IEnumerable<GroupEligibilityRule>? rules)
        {
            var normalized = (rules ?? Enumerable.Empty<GroupEligibilityRule>())
                .Select(rule => new GroupEligibilityRule
                {
                    OfficeLocationId = rule.OfficeLocationId > 0 ? rule.OfficeLocationId : null,
                    DepartmentId = rule.DepartmentId > 0 ? rule.DepartmentId : null
                })
                .DistinctBy(rule => (rule.OfficeLocationId, rule.DepartmentId))
                .ToList();

            if (normalized.Count == 0 ||
                normalized.Any(rule => rule.OfficeLocationId == null && rule.DepartmentId == null))
            {
                return new List<GroupEligibilityRule>
                {
                    new()
                    {
                        OfficeLocationId = null,
                        DepartmentId = null
                    }
                };
            }

            return normalized;
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}
