using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class LeaveDAL
    {
        private readonly PmHrmsContext _context;
        public LeaveDAL(PmHrmsContext context) => _context = context;

       
        public async Task<LeaveRequest> AddLeaveRequest(LeaveRequest leave)
        {
            await _context.LeaveRequests.AddAsync(leave);
            await _context.SaveChangesAsync();                                 
            return leave;                  
        }

        public async Task<LeaveRequest?> GetLeaveById(int leaveId) =>
            await _context.LeaveRequests.FindAsync(leaveId);

        public async Task UpdateLeaveStatus(LeaveRequest leave)
        {
            _context.Entry(leave).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<LeaveRequest>> GetEmployeeLeaves(int employeeId) =>
            await _context.LeaveRequests
                .Include(l => l.LeaveType)
.Include(l => l.ApprovedBy) 
    .ThenInclude(a => a.Designation)
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.AppliedAt)
                .ToListAsync();

        public async Task<List<LeaveRequest>> GetPendingRequests(int orgId) =>
            await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Include(l => l.Employee)
                .Where(l => l.OrganizationId == orgId
                         && l.Status == (int)PmHrmsConstants.LeaveStatus.Pending)
                .ToListAsync();

        public async Task<bool> CheckLeaveTypeExists(int leaveTypeId, int orgId) =>
            await _context.LeaveTypes.AnyAsync(lt =>
                lt.LeaveTypeId == leaveTypeId && lt.OrganizationId == orgId && lt.IsActive);

        public async Task<List<LeaveType>> GetLeaveTypes(int orgId) =>
            await _context.LeaveTypes
                .Where(t => t.OrganizationId == orgId && t.IsActive)
                .OrderBy(t => t.LeaveTypeName)
                .ToListAsync();

        public async Task<LeaveType?> GetLeaveTypeById(int id) =>
            await _context.LeaveTypes.FindAsync(id);

        public async Task<LeaveType> AddLeaveType(LeaveType leaveType)
        {
            await _context.LeaveTypes.AddAsync(leaveType);
            await _context.SaveChangesAsync();
            return leaveType;
        }
        public async Task<List<LeaveRequest>> GetPendingLeaves(int orgId)
        {
            return await _context.LeaveRequests
                .Where(l => l.OrganizationId == orgId && l.Status == 1) 
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Include(l => l.ApprovedBy)
                    .ThenInclude(a => a.Designation)
                .ToListAsync();
        }
        public async Task UpdateLeaveType(LeaveType leaveType)
        {
            _context.Entry(leaveType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLeaveType(int id)
        {
            var type = await _context.LeaveTypes.FindAsync(id);
            if (type != null) { type.IsActive = false; await _context.SaveChangesAsync(); }
        }

    

        // Returns all rows for this org, including leave type + designation data
        public async Task<List<LeaveAllocationRule>> GetAllRuleRows(int orgId) =>
            await _context.LeaveAllocationRules
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRuleDesignations)
                .Where(r => r.OrganizationId == orgId)
                .OrderBy(r => r.RuleName)
                .ToListAsync();

        // All rows belonging to a specific rule_name
        public async Task<List<LeaveAllocationRule>> GetRuleRowsByName(string ruleName, int orgId) =>
            await _context.LeaveAllocationRules
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRuleDesignations)
                .Where(r => r.RuleName == ruleName && r.OrganizationId == orgId)
                .ToListAsync();

        public async Task<LeaveAllocationRule> AddRuleRow(LeaveAllocationRule rule)
        {
            await _context.LeaveAllocationRules.AddAsync(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<Employee?> GetEmployeeById(int empId) =>
        await _context.Employees.FindAsync(empId);

                
        public async Task<Dictionary<string, List<int>>> GetDesignationsByRuleGrouped(int orgId) =>
            (await _context.LeaveRuleDesignations
                .Where(d => d.OrganizationId == orgId)
                .ToListAsync())
            .GroupBy(d => d.RuleName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => d.DesignationId).Distinct().ToList()
            );




        public async Task UpdateRuleRow(LeaveAllocationRule rule)
        {
            _context.Entry(rule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


        public async Task DeleteRuleGroup(string ruleName, int orgId)
        {
           
            var desigs = _context.LeaveRuleDesignations
                .Where(d => d.RuleName == ruleName && d.OrganizationId == orgId);
            _context.LeaveRuleDesignations.RemoveRange(desigs);

            var rows = await _context.LeaveAllocationRules
                .Where(r => r.RuleName == ruleName && r.OrganizationId == orgId)
                .ToListAsync();
            _context.LeaveAllocationRules.RemoveRange(rows);

            await _context.SaveChangesAsync();
        }

        // Unset IsDefault for all rows of this org before setting a new default
        public async Task UnsetDefaultForOrg(int orgId)
        {
            var rows = await _context.LeaveAllocationRules
                .Where(r => r.OrganizationId == orgId && r.IsDefault)
                .ToListAsync();
            foreach (var r in rows) r.IsDefault = false;
            await _context.SaveChangesAsync();
        }

        // Upsert all rows for a rule group:
        //   - Delete existing rows for this ruleName
        //   - Insert fresh rows (one per leave type)
        public async Task UpsertRuleGroup(string ruleName, int orgId, AllocationRuleModel model)
        {
            // Delete existing rows + designation links for this rule_name
            var desigs = _context.LeaveRuleDesignations
                .Where(d => d.RuleName == ruleName && d.OrganizationId == orgId);
            _context.LeaveRuleDesignations.RemoveRange(desigs);

            var existing = await _context.LeaveAllocationRules
                .Where(r => r.RuleName == ruleName && r.OrganizationId == orgId)
                .ToListAsync();
            _context.LeaveAllocationRules.RemoveRange(existing);
            await _context.SaveChangesAsync();

            // Insert one row per leave type item
            foreach (var item in model.Items)
            {
                await _context.LeaveAllocationRules.AddAsync(new LeaveAllocationRule
                {
                    RuleName       = model.RuleName.Trim(),
                    LeaveTypeId    = item.LeaveTypeId,
                    DaysPerMonth   = item.DaysPerMonth,
                    CarryForward   = item.CarryForward,
                    IsDefault      = model.IsDefault,
                    OrganizationId = orgId,
                    EffectiveFrom  = DateOnly.FromDateTime(model.EffectiveFrom),
                    EffectiveTo    = model.EffectiveTo.HasValue
                                         ? DateOnly.FromDateTime(model.EffectiveTo.Value) : null,
                    CreatedAt      = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // Assign designations by rule_name only
            if (model.DesignationIds.Any())
                await AssignDesignationsByName(model.RuleName.Trim(), orgId, model.DesignationIds);
        }


        public async Task AssignDesignationsByName(string ruleName, int orgId, List<int> designationIds)
        {
           
            var duplicates = _context.LeaveRuleDesignations
                .Where(d => d.OrganizationId == orgId &&
                            designationIds.Contains(d.DesignationId));

            _context.LeaveRuleDesignations.RemoveRange(duplicates);

            await _context.SaveChangesAsync();

           
            foreach (var did in designationIds)
            {
                await _context.LeaveRuleDesignations.AddAsync(new LeaveRuleDesignation
                {
                    RuleName = ruleName,
                    DesignationId = did,
                    OrganizationId = orgId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetDesignationsForRule(string ruleName, int orgId) =>
            await _context.LeaveRuleDesignations
                .Where(d => d.RuleName == ruleName && d.OrganizationId == orgId)
                .Select(d => d.DesignationId)
                .Distinct()
                .ToListAsync();


              public async Task<List<LeaveAllocationRule>> GetAllocationRules(int orgId) =>
                    await _context.LeaveAllocationRules
                        .Include(r => r.LeaveType)
                        .Include(r => r.LeaveRuleDesignations)
                        .Where(r => r.OrganizationId == orgId && r.LeaveType.IsActive)
                        .OrderBy(r => r.RuleName)
                        .ToListAsync();
                                                                          
                                     
             public async Task<LeaveRequest?> CancelLeave(int leaveId, int empId)
                        {
                            var leave = await _context.LeaveRequests.FindAsync(leaveId);
                            if (leave == null || leave.EmployeeId != empId) return null;
                            if (leave.Status != (int)PmHrmsConstants.LeaveStatus.Pending) return null;

                            leave.Status   = (int)PmHrmsConstants.LeaveStatus.Cancelled;
                            leave.ActionAt = DateTime.Now;
                            await _context.SaveChangesAsync();
                            return leave;
                        }


            public async Task CarryForwardBalances(int orgId, int fromYear, int toYear)
                {
                    var balances = await _context.LeaveBalance
                        .Include(b => b.LeaveType)
                        .Where(b => b.EmployeeId != 0      // all employees for org
                            && b.Year == fromYear)
                        .Join(_context.Employees.Where(e => e.OrganizationId == orgId && e.IsActive),
                            b => b.EmployeeId,
                            e => e.EmployeeId,
                            (b, e) => b)
                        .ToListAsync();

                    // Rules mein CarryForward flag check karna hai
                    var carryForwardTypeIds = await _context.LeaveAllocationRules
                        .Where(r => r.OrganizationId == orgId && r.CarryForward)
                        .Select(r => r.LeaveTypeId)
                        .Distinct()
                        .ToListAsync();

                    // Group by employee + leaveType to compute net balance
                    var grouped = balances
                        .GroupBy(b => new { b.EmployeeId, b.LeaveTypeId })
                        .ToList();

                    foreach (var g in grouped)
                    {
                        var leaveTypeId = g.Key.LeaveTypeId;

                        // Agar CarryForward nahi hai toh skip (reset automatically — no entry means 0)
                        if (!carryForwardTypeIds.Contains(leaveTypeId)) continue;

                        var unused = g.Sum(b => b.Balance - b.Used - b.PreDeducted);
                        if (unused <= 0) continue;

                        // Check if entry already exists for new year month=1
                        var exists = await _context.LeaveBalance.AnyAsync(b =>
                            b.EmployeeId  == g.Key.EmployeeId &&
                            b.LeaveTypeId == leaveTypeId &&
                            b.Month       == 1 &&
                            b.Year        == toYear);

                        if (exists) continue;

                        await _context.LeaveBalance.AddAsync(new LeaveBalance
                        {
                            EmployeeId     = g.Key.EmployeeId,
                            LeaveTypeId    = leaveTypeId,
                            Month          = 0,           // Month=0 = carry-forward entry (special)
                            Year           = toYear,
                            Balance        = unused,
                            Used           = 0,
                            PreDeducted    = 0,
                            CreatedAt      = DateTime.UtcNow
                        });
                    }

                    await _context.SaveChangesAsync();
                }

        // Find applicable rule for an employee (by designation or default)
        public async Task<List<LeaveAllocationRule>> GetApplicableRuleRows(int? designationId, int orgId)
        {
            if (designationId.HasValue)
            {
                // Find rule_name assigned to this designation
                var ruleName = await _context.LeaveRuleDesignations
                    .Where(d => d.DesignationId == designationId.Value && d.OrganizationId == orgId)
                    .Select(d => d.RuleName)
                    .FirstOrDefaultAsync();

                if (ruleName != null)
                    return await _context.LeaveAllocationRules
                        .Include(r => r.LeaveType)
                        .Where(r => r.RuleName == ruleName && r.OrganizationId == orgId)
                        .ToListAsync();
            }

            // Fallback: default rule
            var defaultRuleName = await _context.LeaveAllocationRules
                .Where(r => r.OrganizationId == orgId && r.IsDefault)
                .Select(r => r.RuleName)
                .FirstOrDefaultAsync();

            if (defaultRuleName == null) return new List<LeaveAllocationRule>();

            return await _context.LeaveAllocationRules
                .Include(r => r.LeaveType)
                .Where(r => r.RuleName == defaultRuleName && r.OrganizationId == orgId)
                .ToListAsync();
        }

        // ==============================================================
        // LEAVE BALANCE
        // ==============================================================
        public async Task<List<LeaveBalance>> GetEmployeeBalances(int employeeId, int year) =>
            await _context.LeaveBalance
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == employeeId && b.Year == year)
                .ToListAsync();

        public async Task<LeaveBalance?> GetBalance(int empId, int ltId, int month, int year) =>
            await _context.LeaveBalance.FirstOrDefaultAsync(b =>
                b.EmployeeId == empId && b.LeaveTypeId == ltId && b.Month == month && b.Year == year);

        public async Task<decimal> GetAvailableBalance(int empId, int ltId, int year)
        {
            var rows = await _context.LeaveBalance
                .Where(b => b.EmployeeId == empId && b.LeaveTypeId == ltId && b.Year == year)
                .ToListAsync();
            return rows.Sum(b => b.Balance - b.Used - b.PreDeducted);
        }

       public async Task UpsertBalance(LeaveBalance balance)
            {
                var existing = await GetBalance(
                    balance.EmployeeId, balance.LeaveTypeId, balance.Month, balance.Year);

                if (existing == null)
                    await _context.LeaveBalance.AddAsync(balance);
                else
                {
                    existing.RuleSnapshotId = balance.RuleSnapshotId;
                    
                }

                await _context.SaveChangesAsync();
            }

        public async Task AddPreDeducted(int empId, int ltId, decimal days, int year)
        {
            var latest = await _context.LeaveBalance
                .Where(b => b.EmployeeId == empId && b.LeaveTypeId == ltId && b.Year == year)
                .OrderByDescending(b => b.Month).FirstOrDefaultAsync();
            if (latest != null) { latest.PreDeducted += days; await _context.SaveChangesAsync(); }
        }

        public async Task FinalizeLeaveBalance(int empId, int ltId, decimal days, int year, bool approved)
        {
            var latest = await _context.LeaveBalance
                .Where(b => b.EmployeeId == empId && b.LeaveTypeId == ltId && b.Year == year)
                .OrderByDescending(b => b.Month).FirstOrDefaultAsync();
            if (latest != null)
            {
                latest.PreDeducted = Math.Max(latest.PreDeducted - days, 0);
                if (approved) latest.Used += days;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Employee>> GetActiveEmployees(int orgId) =>
            await _context.Employees.Where(e => e.OrganizationId == orgId && e.IsActive).ToListAsync();

        public async Task<List<int>> GetAllOrgIds() =>
            await _context.Organizations.Where(o => o.IsActive == true).Select(o => o.OrgId).ToListAsync();

          public async Task<List<Employee>> GetAllActiveEmployees() =>
            await _context.Employees
                .IgnoreQueryFilters()
                .Where(e => e.IsActive)
                .ToListAsync();
        
            

            public async Task<List<LeaveAllocationRule>> GetAllAllocationRules() =>
            await _context.LeaveAllocationRules
                .IgnoreQueryFilters()
                .Include(r => r.LeaveType)
                .Include(r => r.LeaveRuleDesignations)
                .Where(r => r.LeaveType.IsActive)
                .ToListAsync();

          
          public async Task BulkUpsertBalances(List<LeaveBalance> balances)
            {
                foreach (var balance in balances)
                {
                    var existing = await _context.LeaveBalance.FirstOrDefaultAsync(b =>
                        b.EmployeeId  == balance.EmployeeId  &&
                        b.LeaveTypeId == balance.LeaveTypeId &&
                        b.Month       == balance.Month       &&
                        b.Year        == balance.Year);

                    if (existing == null)
                        await _context.LeaveBalance.AddAsync(balance);
                    else
                        existing.RuleSnapshotId = balance.RuleSnapshotId;
                }

                await _context.SaveChangesAsync(); 
            }
        public async Task<List<LeaveMasterResponseModel>> GetActiveLeaveMasters()
        {
            return await _context.LeaveMasters
                .Where(x => x.IsActive)
                .Select(x => new LeaveMasterResponseModel
                {
                    LeaveMasterId = x.LeaveMasterId,
                    LeaveTypeName = x.LeaveTypeName,
                    MaxDaysPerApplication = x.MaxDaysPerApplication,
                    IsBalanceBased = x.IsBalanceBased,
                    IsSpecialPolicy = x.IsSpecialPolicy
                }).ToListAsync();
        }
        public async Task<LeaveType> CreateLeaveType(LeaveTypeModel model, int orgId)
        {
            if (model.LeaveMasterId.HasValue)
            {
                bool exists = await _context.LeaveTypes
                    .AnyAsync(x => x.OrganizationId == orgId && x.LeaveMasterId == model.LeaveMasterId && x.IsActive);
                if (exists) throw new Exception("This leave type is already added for your organization.");
            }

            var newType = new LeaveType
            {
                LeaveMasterId = model.LeaveMasterId,
                LeaveTypeName = model.LeaveTypeName.Trim(),
                OrganizationId = orgId,
                IsActive = true,
                MaxDaysPerApplication = model.MaxDaysPerApplication,
                IsBalanceBased = model.IsBalanceBased,
                IsSpecialPolicy = model.IsSpecialPolicy,
                CreatedAt = DateTime.UtcNow
            };

            await _context.LeaveTypes.AddAsync(newType);
            await _context.SaveChangesAsync();
            return newType;
        }


    }

}