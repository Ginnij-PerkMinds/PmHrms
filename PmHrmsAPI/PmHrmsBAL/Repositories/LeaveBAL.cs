using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using Microsoft.EntityFrameworkCore;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{   
    public class LeaveBAL : ILeaveBAL
    {            
        private readonly LeaveDAL _leaveDAL;

         //private readonly PmHrmsContext _context;     
         private readonly IPermissionService _permissionService;
                                    
        private readonly ILogger<LeaveBAL> _logger;  

        public LeaveBAL(LeaveDAL leaveDAL, PmHrmsContext context, IPermissionService permissionService,
                    ILogger<LeaveBAL> logger)     
        {
            _leaveDAL = leaveDAL;
            //_context = context;
            _permissionService = permissionService;
            _logger = logger;                        
        }

        
        public async Task<LeaveRequest> ApplyLeave(LeaveApplyModel model, int empId, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPLY);

            if (!await _leaveDAL.CheckLeaveTypeExists(model.LeaveTypeId, orgId))
                //throw new ArgumentException("Invalid Leave Type.");                              
                throw new ArgumentException(PmHrmsConstants.LeaveMessages.InvalidLeaveType);
            var leaveType = await _leaveDAL.GetLeaveTypeById(model.LeaveTypeId)
                //?? throw new ArgumentException("Leave type not found.");
                ?? throw new ArgumentException(PmHrmsConstants.LeaveMessages.LeaveTypeNotFound);

            var fromDate  = DateOnly.FromDateTime(model.FromDate);
            var toDate    = DateOnly.FromDateTime(model.ToDate);
            var totalDays = (decimal)(model.ToDate.Date - model.FromDate.Date).TotalDays + 1;

            if (toDate < fromDate)
                //throw new ArgumentException("To date cannot be before from date.");
                throw new ArgumentException(PmHrmsConstants.LeaveMessages.ToDateBeforeFromDate);

            if (leaveType.MaxDaysPerApplication.HasValue && totalDays > leaveType.MaxDaysPerApplication.Value)
                //throw new ArgumentException($"Max {leaveType.MaxDaysPerApplication} days allowed.");
                throw new ArgumentException(string.Format(PmHrmsConstants.LeaveMessages.MaxDaysAllowed, leaveType.MaxDaysPerApplication.Value));

            if (leaveType.IsBalanceBased)
            {
                var available = await _leaveDAL.GetAvailableBalance(empId, model.LeaveTypeId, DateTime.Now.Year);
                if (available < totalDays)
                    //throw new InvalidOperationException(
                    //    $"Insufficient balance. Available: {available}, Requested: {totalDays}.");
                    throw new InvalidOperationException(string.Format(PmHrmsConstants.LeaveMessages.InsufficientBalance, available, totalDays));
                await _leaveDAL.AddPreDeducted(empId, model.LeaveTypeId, totalDays, DateTime.Now.Year);
            }

            return await _leaveDAL.AddLeaveRequest(new LeaveRequest
            {
                EmployeeId = empId, OrganizationId = orgId,
                LeaveTypeId = model.LeaveTypeId, FromDate = fromDate, ToDate = toDate,
                TotalDays = totalDays, Status = (int)PmHrmsConstants.LeaveStatus.Pending,
                AppliedAt = DateTime.Now, Reason = model.Reason
            });
        }

        public async Task<bool> ApproveOrRejectLeave(LeaveApprovalModel model, int approvedById)
        {
            var request = await _leaveDAL.GetLeaveById(model.LeaveId);
            if (request == null) return false;

            _permissionService.EnsureCanActOn(PermissionKeys.LV_APPROVE_DENY, request.EmployeeId);

            var prevStatus       = request.Status;
            request.Status       = (int)model.Status;
            request.ApprovedById = _permissionService.GetCurrentEmployeeId();
            request.ActionAt     = DateTime.Now;
            await _leaveDAL.UpdateLeaveStatus(request);

            if (prevStatus == (int)PmHrmsConstants.LeaveStatus.Pending)
            {
                var lt = await _leaveDAL.GetLeaveTypeById(request.LeaveTypeId);
                if (lt?.IsBalanceBased == true)
                {
                    bool approved = request.Status == (int)PmHrmsConstants.LeaveStatus.Approved;
                    await _leaveDAL.FinalizeLeaveBalance(
                        request.EmployeeId, request.LeaveTypeId,
                        request.TotalDays, request.FromDate.Year, approved);
                }
            }
            return true;
        }



        public async Task<List<LeaveResponseModel>> GetMyLeaves(int empId) =>
            (await _leaveDAL.GetEmployeeLeaves(empId)).Select(l => new LeaveResponseModel
            {
                LeaveId = l.LeaveId,
                LeaveTypeName = l.LeaveType.LeaveTypeName,

                FromDate = l.FromDate.ToDateTime(TimeOnly.MinValue),
                ToDate = l.ToDate.ToDateTime(TimeOnly.MinValue),

                TotalDays = l.TotalDays,
                Status = l.Status.ToString(),
                Reason = l.Reason,
                AppliedAt = l.AppliedAt,

                
                ApprovedBy = l.ApprovedBy != null
                    ? l.ApprovedBy.FirstName + " " + l.ApprovedBy.LastName +
                      (l.ApprovedBy.Designation != null
                        ? " (" + l.ApprovedBy.Designation.DesignationName + ")"
                        : "")
                    : null
            }).ToList();

       
        public async Task<List<LeaveTypeResponseModel>> GetLeaveTypes(int orgId) =>
            (await _leaveDAL.GetLeaveTypes(orgId)).Select(t => new LeaveTypeResponseModel
            {
                LeaveTypeId = t.LeaveTypeId, LeaveTypeName = t.LeaveTypeName,
                MaxDaysPerApplication = t.MaxDaysPerApplication,
                IsBalanceBased = t.IsBalanceBased, IsSpecialPolicy = t.IsSpecialPolicy,
                IsActive = t.IsActive
            }).ToList();

        public async Task<List<LeaveMasterResponseModel>> GetLeaveMasterList()
        {
            return await _leaveDAL.GetActiveLeaveMasters();
                //.Where(x => x.IsActive)
                //.Select(x => new LeaveMasterResponseModel
                //{
                //    LeaveMasterId = x.LeaveMasterId,
                //    LeaveTypeName = x.LeaveTypeName,
                //    MaxDaysPerApplication = x.MaxDaysPerApplication,
                //    IsBalanceBased = x.IsBalanceBased,
                //    IsSpecialPolicy = x.IsSpecialPolicy
                //}).ToListAsync();
        }
        public async Task<LeaveType> CreateLeaveType(LeaveTypeModel model, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);

            //if (model.LeaveMasterId.HasValue)
            //{
            //    bool exists = await _context.LeaveTypes.AnyAsync(x => x.OrganizationId == orgId && x.LeaveMasterId == model.LeaveMasterId && x.IsActive);
            //    if (exists) throw new Exception("This leave type is already added for your organization.");
            //}

            //var newType = new LeaveType
            //{
            //    LeaveMasterId = model.LeaveMasterId,
            //    LeaveTypeName = model.LeaveTypeName.Trim(),
            //    OrganizationId = orgId,
            //    IsActive = true,
            //    MaxDaysPerApplication = model.MaxDaysPerApplication,
            //    IsBalanceBased = model.IsBalanceBased,
            //    IsSpecialPolicy = model.IsSpecialPolicy,
            //    CreatedAt = DateTime.UtcNow
            //};

            //await _context.LeaveTypes.AddAsync(newType);
            //await _context.SaveChangesAsync();
            return await _leaveDAL.CreateLeaveType(model, orgId);
           
        }

        public async Task UpdateLeaveType(int id, LeaveTypeModel model, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
            var type = await _leaveDAL.GetLeaveTypeById(id)
                //?? throw new ArgumentException("Not found.");
                ?? throw new ArgumentException(PmHrmsConstants.LeaveMessages.LeaveTypeNotFound);
            if (type.OrganizationId != orgId) 
                throw new UnauthorizedAccessException();
            type.LeaveTypeName = model.LeaveTypeName.Trim();
            type.MaxDaysPerApplication = model.MaxDaysPerApplication;
            type.IsBalanceBased = model.IsBalanceBased;
            type.IsSpecialPolicy = model.IsSpecialPolicy;
            await _leaveDAL.UpdateLeaveType(type);
        }

        public async Task DeleteLeaveType(int id)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
            await _leaveDAL.DeleteLeaveType(id);
        }


        public async Task<List<AllocationRuleResponseModel>> GetAllocationRules(int orgId)
        {
            try
            {
                _logger.LogInformation("[LeaveBAL:GetAllocationRules] START — OrgId={OrgId}", orgId);

                _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
                _logger.LogDebug("[LeaveBAL:GetAllocationRules] Permission check passed");

                var rows = await _leaveDAL.GetAllRuleRows(orgId);
                _logger.LogDebug("[LeaveBAL:GetAllocationRules] Raw rows fetched — Count={Count}", rows.Count);

                  var desigMap = await _leaveDAL.GetDesignationsByRuleGrouped(orgId);
                    _logger.LogDebug("[LeaveBAL:GetAllocationRules] DesigMap keys={Keys}",
                  string.Join(", ", desigMap.Keys));


                var result = rows
                    .GroupBy(r => r.RuleName)
                    .Select(g =>
                    {
                        var first = g.First();


                        var assignedIds = desigMap.TryGetValue(g.Key, out var ids)
                    ? ids
                    : new List<int>();

                    _logger.LogDebug(
                        "[LeaveBAL:GetAllocationRules] Rule={Name} Items={Items} Desigs={Desigs}",
                        g.Key, g.Count(), assignedIds.Count);


                        return new AllocationRuleResponseModel
                        {
                            RuleName = g.Key,
                            IsDefault = first.IsDefault,
                            EffectiveFrom = first.EffectiveFrom,
                            EffectiveTo = first.EffectiveTo,
                            Items = g.Select(r => new RuleItemResponseModel
                            {
                                RuleId = r.RuleId,
                                LeaveTypeId = r.LeaveTypeId,
                                LeaveTypeName = r.LeaveType?.LeaveTypeName ?? "[NULL]",  
                                DaysPerMonth = r.DaysPerMonth,
                                CarryForward = r.CarryForward
                            }).ToList(),
                           AssignedDesignationIds = assignedIds
                        };
                    }).ToList();

                _logger.LogInformation(
                    "[LeaveBAL:GetAllocationRules] SUCCESS — OrgId={OrgId} Rules={Count}",
                    orgId, result.Count);

                return result;
            }
            catch (Exception ex)
            {
               _logger.LogError(ex,
                    "[LeaveBAL:GetAllocationRules] FAILED — OrgId={OrgId} | ExType={ExType} | Msg={Msg}",
                    orgId, ex.GetType().Name, ex.Message);
                throw;
            }
        }

public async Task<AllocationRuleResponseModel> CreateAllocationRule(AllocationRuleModel model, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
            if (!model.Items.Any())
                //throw new ArgumentException("Add at least one leave type.");
                throw new ArgumentException(PmHrmsConstants.LeaveMessages.AddAtLeastOneLeaveType);

            if (model.IsDefault) await _leaveDAL.UnsetDefaultForOrg(orgId);
            await _leaveDAL.UpsertRuleGroup(model.RuleName.Trim(), orgId, model);

            return (await GetAllocationRules(orgId))
                .First(r => r.RuleName == model.RuleName.Trim());
        }

        public async Task<AllocationRuleResponseModel> UpdateAllocationRule(string ruleName, AllocationRuleModel model, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
            if (!model.Items.Any()) throw new ArgumentException("Add at least one leave type.");

            if (model.IsDefault) await _leaveDAL.UnsetDefaultForOrg(orgId);

            
            await _leaveDAL.DeleteRuleGroup(ruleName, orgId);   
            await _leaveDAL.UpsertRuleGroup(model.RuleName.Trim(), orgId, model);

            return (await GetAllocationRules(orgId))
                .First(r => r.RuleName == model.RuleName.Trim());                                                                                                                                                                               
        }       

        public async Task DeleteAllocationRule(string ruleName, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);                                                                                                                                                           
            await _leaveDAL.DeleteRuleGroup(ruleName, orgId);           
        }

        public async Task AssignDesignations(string ruleName, List<int> designationIds, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.LV_APPROVE_DENY);
            await _leaveDAL.AssignDesignationsByName(ruleName, orgId, designationIds);
        }

        public async Task<List<LeaveResponseModel>> GetPendingRequests(int orgId)
        {
            var leaves = await _leaveDAL.GetPendingLeaves(orgId);

            return leaves.Select(l => new LeaveResponseModel
            {
                LeaveId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                LeaveTypeName = l.LeaveType.LeaveTypeName,

                FromDate = l.FromDate.ToDateTime(TimeOnly.MinValue),
                ToDate = l.ToDate.ToDateTime(TimeOnly.MinValue),

                TotalDays = l.TotalDays,
                Status = l.Status.ToString(),
                Reason = l.Reason,
                AppliedAt = l.AppliedAt,

              
                ApprovedBy = l.ApprovedBy != null
                    ? l.ApprovedBy.FirstName + " " + l.ApprovedBy.LastName +
                      (l.ApprovedBy.Designation != null
                        ? " (" + l.ApprovedBy.Designation.DesignationName + ")"
                        : "")
                    : null
            }).ToList();
        }


        public async Task<List<LeaveBalanceResponseModel>> GetLeaveBalance(int empId)
{
    var rows = await _leaveDAL.GetEmployeeBalances(empId, DateTime.Now.Year);
    if (!rows.Any()) return new List<LeaveBalanceResponseModel>();

    int currentMonth = DateTime.Now.Month;

    
    var emp         = await _leaveDAL.GetEmployeeById(empId);         
    var ruleRows    = await _leaveDAL.GetApplicableRuleRows(emp?.DesignationId, emp?.OrganizationId ?? 0);

   
    var carryMap = ruleRows.ToDictionary(r => r.LeaveTypeId, r => r.CarryForward);

    return rows
        .GroupBy(b => new { b.LeaveTypeId, b.LeaveType.LeaveTypeName, b.LeaveType.IsBalanceBased })
        .Select(g =>
        {
            bool carryForward = carryMap.TryGetValue(g.Key.LeaveTypeId, out var cf) && cf;

            decimal balance, used, preDeducted;

            if (carryForward)
            {
                // Jan se current month tak sab sum karo
                var relevantRows = g.Where(b => b.Month <= currentMonth || b.Month == 0);
                balance     = relevantRows.Sum(b => b.Balance);
                used        = relevantRows.Sum(b => b.Used);
                preDeducted = relevantRows.Sum(b => b.PreDeducted);
            }
            else
            {
                // ✅ Sirf current month — previous unused LOST (correct for non-carry types)
                var cur     = g.FirstOrDefault(b => b.Month == currentMonth);
                balance     = cur?.Balance     ?? 0;
                used        = cur?.Used        ?? 0;
                preDeducted = cur?.PreDeducted ?? 0;
            }

            return new LeaveBalanceResponseModel
            {
                LeaveTypeId         = g.Key.LeaveTypeId,
                LeaveTypeName       = g.Key.LeaveTypeName,
                IsBalanceBased      = g.Key.IsBalanceBased,
                Balance             = balance,
                Used                = used,
                PreDeducted         = preDeducted,
                CarryForwardBalance = g.Where(b => b.Month == 0).Sum(b => b.Balance)
            };
        }).ToList();
}

    public async Task<bool> CancelLeave(int leaveId, int empId)
        {
            var leave = await _leaveDAL.CancelLeave(leaveId, empId);
            if (leave == null) return false;

            
            var lt = await _leaveDAL.GetLeaveTypeById(leave.LeaveTypeId);
            if (lt?.IsBalanceBased == true)
            {
                await _leaveDAL.FinalizeLeaveBalance(
                    empId, leave.LeaveTypeId,
                    leave.TotalDays, leave.FromDate.Year,
                    approved: false);   
            }         
            return true;                                               
        }
    }                       
                             
}                                  
             