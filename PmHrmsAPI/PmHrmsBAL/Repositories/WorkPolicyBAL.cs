using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using Microsoft.EntityFrameworkCore;               
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;
using System.Reflection.Metadata.Ecma335;

public class WorkPolicyBAL : IWorkPolicyBAL
{          
    private readonly WorkPolicyDAL _dal;
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<WorkPolicyBAL> _logger;                  
    private readonly ITenantService _tenantService;                     
    //private readonly PmHrmsContext _context;
    private readonly DesignationWorkPolicyDAL _designationPolicyDal;      

    public WorkPolicyBAL(
        WorkPolicyDAL dal,
        IPermissionService permissionService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<WorkPolicyBAL> logger,
        ITenantService tenantService,
        //PmHrmsContext context,
        DesignationWorkPolicyDAL designationPolicyDal 
    )
    {
        _dal = dal;
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _tenantService = tenantService;
        //_context = context;
        _designationPolicyDal = designationPolicyDal; 
    }

    public async Task<PagedResult<WorkPolicyResponseModel>> GetAll(int page, int size, string? search)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_VIEW);
        var orgId = _tenantService.GetOrgId();

        var (entities, count) = await _dal.GetAll(page, size, search, orgId);

        var items = entities.Select(MapToResponse).ToList();

        return new PagedResult<WorkPolicyResponseModel>
        {
            Items = items,
            TotalCount = count,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<WorkPolicyResponseModel?> GetById(int id)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_VIEW);
        var p = await _dal.GetById(id);
        if (p == null) return null;

        return MapToResponse(p);
    }

    public async Task<WorkPolicyResponseModel> Create(WorkPolicyModel model)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_CREATE);

        ApplyDerivedValues(model);
        ValidateWorkPolicy(model);

        
        if (model.IsDefault)
        {
            await RemoveExistingDefaultPolicy();
        }

       
        var entity = new WorkPolicy
        {
            PolicyName = model.PolicyName.Trim(),
            RequiredWorkingMinutes = model.RequiredWorkingMinutes,
            LateAfterMinutes = model.LateAfterMinutes,
            HalfDayThresholdMinutes = model.HalfDayThresholdMinutes,
            IsWfhAllowed = model.IsWfhAllowed,
            IsWfoRequired = model.IsWfoRequired,
            OrganizationId = _tenantService.GetOrgId(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ShiftStartTime = model.ShiftStartTime,
            ShiftEndTime = model.ShiftEndTime,
            BreakStartTime = model.BreakStartTime,
            BreakEndTime = model.BreakEndTime,
            IsFlexibleShift = model.IsFlexibleShift,
            AdditionalBreakMinutes = model.AdditionalBreakMinutes,
            MaxBreakMinutes = model.MaxBreakMinutes,
            MaxBreakCount = model.MaxBreakCount,
            IsBreakPaid = model.IsBreakPaid,
            IsDefault = model.IsDefault,
            WeekOffs = BuildWeekOffEntities(model.WeekOffs)
        };

        var created = await _dal.Add(entity);

        return MapToResponse(created);
    }

    public async Task<WorkPolicyResult?> GetWorkPolicyByEmployeeId(int employeeId)
    {
        try
        {
            var employee = await _dal.GetEmployeeById(employeeId);

            if (employee == null)
                return null; // Changed from throw to return null

            var orgId = _tenantService.GetOrgId();

            // Employee-level policy
            if (employee.PolicyId != null)
            {
                var empPolicy = await _dal.GetEmployeePolicy(employee.PolicyId.Value);

                if (empPolicy != null)
                    return empPolicy;
            }

            // Designation-level policy
            if (employee.DesignationId != null)
            {
                var result = await _dal.GetDesignationPolicy(employee.DesignationId.Value);
                if (result != null)
                    return result;
            }

            // Default policy
            return await _dal.GetDefaultPolicy(orgId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work policy for employee {EmployeeId}", employeeId);
            return null; // Return null on error instead of throwing
        }
    }


    public async Task<List<DesignationPolicyMappingResponse>> GetDesignationPolicyMappings()
    {
        return await _designationPolicyDal.GetMappings();
    }

    public async Task AssignPolicyToDesignation(int designationId, int policyId)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_EDIT);


     //   var designationExists = await _context.Designations
     //.AnyAsync(d => d.DesignationId == designationId);
     var designationExists = await _dal.DesignationExists(designationId);

        if (!designationExists)
            //throw new Exception("Invalid Designation");
            throw new Exception(PmHrmsConstants.WorkPolicyMessages.InvalidDesignation);

        await _designationPolicyDal.AddOrUpdate(designationId, policyId);
    }

    public async Task<WorkPolicyResponseModel?> Update(int id, WorkPolicyModel model)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_EDIT);

        var entity = await _dal.GetById(id);
        if (entity == null) return null;

        ApplyDerivedValues(model);
        ValidateWorkPolicy(model);

        entity.PolicyName = model.PolicyName.Trim();
        entity.RequiredWorkingMinutes = model.RequiredWorkingMinutes;
        entity.LateAfterMinutes = model.LateAfterMinutes;
        entity.HalfDayThresholdMinutes = model.HalfDayThresholdMinutes;
        entity.IsWfhAllowed = model.IsWfhAllowed;
        entity.IsWfoRequired = model.IsWfoRequired;
        entity.IsActive = model.IsActive;
        entity.ShiftStartTime = model.ShiftStartTime;
        entity.ShiftEndTime = model.ShiftEndTime;
        entity.BreakStartTime = model.BreakStartTime;
        entity.BreakEndTime = model.BreakEndTime;
        entity.IsFlexibleShift = model.IsFlexibleShift;
        entity.AdditionalBreakMinutes = model.AdditionalBreakMinutes;
        entity.MaxBreakMinutes = model.MaxBreakMinutes;
        entity.MaxBreakCount = model.MaxBreakCount;
        entity.IsBreakPaid = model.IsBreakPaid;
        SyncWeekOffs(entity, model.WeekOffs);


        if (model.IsDefault)
        {
            await RemoveExistingDefaultPolicy();
            entity.IsDefault = true;
        }
        else
        {
          
            entity.IsDefault = entity.IsDefault;
        }
      

        await _dal.Update(entity);
        return await GetById(id);
    }

    private void ValidateWorkPolicy(WorkPolicyModel model)
    {
        if (model.RequiredWorkingMinutes <= 0)
            //throw new ArgumentException("Required working minutes must be greater than zero");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.RequiredMinutes);

        if (model.LateAfterMinutes >= model.RequiredWorkingMinutes)
            //throw new ArgumentException("Late minutes cannot exceed working minutes");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.LateMinutesExceed);

        if (model.HalfDayThresholdMinutes > model.RequiredWorkingMinutes)
            //throw new ArgumentException("Half-day threshold cannot exceed required working minutes");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.HalfDayThresholdExceed);

        var hasBreakStart = model.BreakStartTime != null;
        var hasBreakEnd = model.BreakEndTime != null;
        if (hasBreakStart != hasBreakEnd)     
            //throw new ArgumentException("Both break start and break end time are required.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.BreakStartEndRequired);
                                 
        if (hasBreakStart && model.BreakStartTime >= model.BreakEndTime)  
            //throw new ArgumentException("Break start time must be before break end time.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.BreakStartBeforeEnd);
                                                            
        if (!model.IsFlexibleShift)
        {                        
            if (model.ShiftStartTime == null || model.ShiftEndTime == null)
                //throw new ArgumentException("Shift timings are required for fixed shifts.");
                throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.ShiftTimingsRequired);

            if (model.ShiftStartTime >= model.ShiftEndTime)
                //throw new ArgumentException("Shift start time must be before end time.");
                throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.ShiftStartBeforeEnd);

            if (hasBreakStart &&
                (model.BreakStartTime < model.ShiftStartTime || model.BreakEndTime > model.ShiftEndTime))
            {
                //throw new ArgumentException("Break timings must fall within the shift timings.");
                throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.BreakWithinShift);
            }
        }

        if (model.MaxBreakMinutes > 0 && model.MaxBreakCount <= 0)
            //throw new ArgumentException("Break count must be at least 1 when break time is configured.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.BreakCountRequired);

        ValidateWeekOffs(model.WeekOffs);
    }
    private async Task RemoveExistingDefaultPolicy()
    {
        var orgId = _tenantService.GetOrgId();

    //var existingDefaults = await _context.WorkPolicies
    //    .Where(x => x.OrganizationId == orgId && x.IsDefault)
    //    .ToListAsync();


    //if (!existingDefaults.Any())
    //    return;

    //foreach (var policy in existingDefaults)
    //{
    //    policy.IsDefault = false;
    //}

    //await _context.SaveChangesAsync();
    await _dal.UnsetDefaultPolicies(orgId);
}
    private static void ApplyDerivedValues(WorkPolicyModel model)
    {
        
        var scheduledBreakMinutes = GetScheduledBreakMinutes(model.BreakStartTime, model.BreakEndTime);
        var derivedBreakMinutes = scheduledBreakMinutes + Math.Max(model.AdditionalBreakMinutes, 0);
        model.MaxBreakMinutes = derivedBreakMinutes > 0
            ? derivedBreakMinutes
            : Math.Max(model.MaxBreakMinutes, 0);

        if (model.IsFlexibleShift || model.ShiftStartTime == null || model.ShiftEndTime == null)
            return;

        if (model.ShiftEndTime <= model.ShiftStartTime)
            return;

       
        var shiftMinutes = (int)(model.ShiftEndTime.Value - model.ShiftStartTime.Value).TotalMinutes;
        model.RequiredWorkingMinutes = model.IsBreakPaid
            ? shiftMinutes
            : Math.Max(shiftMinutes - scheduledBreakMinutes, 1);
    }

    public async Task RemovePolicyFromDesignation(int designationId, int policyId)
    {
    //var mapping = await _context.DesignationWorkPolicyMappings
    //    .FirstOrDefaultAsync(x =>
    //        x.DesignationId == designationId &&
    //        x.WorkPolicyId == policyId); 

    //if (mapping != null)
    //{
    //    _context.DesignationWorkPolicyMappings.Remove(mapping);
    //    await _context.SaveChangesAsync();
    //}
    await _dal.RemoveMapping(designationId, policyId);
}

    private static int GetScheduledBreakMinutes(TimeOnly? breakStartTime, TimeOnly? breakEndTime)
    {
        if (breakStartTime == null || breakEndTime == null || breakEndTime <= breakStartTime)
            return 0;

        return (int)(breakEndTime.Value - breakStartTime.Value).TotalMinutes;
    }

    private static WorkPolicyResponseModel MapToResponse(WorkPolicy policy)
    {
        return new WorkPolicyResponseModel
        {
            PolicyId = policy.PolicyId,
            PolicyName = policy.PolicyName,
            RequiredWorkingMinutes = policy.RequiredWorkingMinutes,
            LateAfterMinutes = policy.LateAfterMinutes,
            HalfDayThresholdMinutes = policy.HalfDayThresholdMinutes,
            IsWfhAllowed = policy.IsWfhAllowed,
            IsWfoRequired = policy.IsWfoRequired,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            ShiftStartTime = policy.ShiftStartTime,
            ShiftEndTime = policy.ShiftEndTime,
            BreakStartTime = policy.BreakStartTime,
            BreakEndTime = policy.BreakEndTime,
            IsFlexibleShift = policy.IsFlexibleShift,
            AdditionalBreakMinutes = policy.AdditionalBreakMinutes,
            MaxBreakMinutes = policy.MaxBreakMinutes,
            MaxBreakCount = policy.MaxBreakCount,
            IsBreakPaid = policy.IsBreakPaid,
            IsDefault = policy.IsDefault,
            WeekOffs = policy.WeekOffs
                .OrderBy(GetWeekOffSortOrder)
                .Select(weekOff => new WorkPolicyWeekOffResponseModel
                {
                    DayOfWeek = weekOff.DayOfWeek,
                    IsHalfDay = weekOff.IsHalfDay
                })
                .ToList()
        };
    }

    private static List<WorkPolicyWeekOff> BuildWeekOffEntities(IEnumerable<WorkPolicyWeekOffModel>? weekOffs, int? policyId = null)
    {
        return (weekOffs ?? Enumerable.Empty<WorkPolicyWeekOffModel>())
            .OrderBy(GetWeekOffSortOrder)
            .Select(weekOff =>
            {
                var entity = new WorkPolicyWeekOff
                {
                    DayOfWeek = weekOff.DayOfWeek,
                    IsHalfDay = weekOff.IsHalfDay,
                    CreatedAt = DateTime.UtcNow
                };

                if (policyId.HasValue)
                {
                    entity.PolicyId = policyId.Value;
                }

                return entity;
            })
            .ToList();
    }

    private void SyncWeekOffs(WorkPolicy entity, IEnumerable<WorkPolicyWeekOffModel>? weekOffs)
    {
    //if (entity.WeekOffs.Any())
    //{
    //    _context.WorkPolicyWeekOffs.RemoveRange(entity.WeekOffs.ToList());
    //    entity.WeekOffs.Clear();
    //}
    _dal.SyncWeekOffs(entity, weekOffs);


    foreach (var weekOff in BuildWeekOffEntities(weekOffs, entity.PolicyId))
        {
            entity.WeekOffs.Add(weekOff);
        }
    }

    private static void ValidateWeekOffs(IEnumerable<WorkPolicyWeekOffModel>? weekOffs)
    {
        if (weekOffs == null)
        {
            return;
        }

        var weekOffList = weekOffs.ToList();
        if (weekOffList.Count > 7)
        {
            //throw new ArgumentException("A work policy can only have up to 7 week offs.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.MaxWeekOffsExceeded);
        }

        var invalidDay = weekOffList.FirstOrDefault(weekOff => !Enum.IsDefined(typeof(DayOfWeek), weekOff.DayOfWeek));
        if (invalidDay != null)
        {
            //throw new ArgumentException("One or more selected week offs are invalid.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.InvalidWeekOffs);
        }

        var duplicateDay = weekOffList
            .GroupBy(weekOff => weekOff.DayOfWeek)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateDay != null)
        {
            //throw new ArgumentException($"{duplicateDay.Key} is selected more than once as a week off.");
            throw new ArgumentException(PmHrmsConstants.WorkPolicyMessages.DuplicateWeekOff);
        }
    }

    private static int GetWeekOffSortOrder(WorkPolicyWeekOff weekOff)
    {
        return GetWeekOffSortOrder(weekOff.DayOfWeek);
    }

    private static int GetWeekOffSortOrder(WorkPolicyWeekOffModel weekOff)
    {
        return GetWeekOffSortOrder(weekOff.DayOfWeek);
    }

    private static int GetWeekOffSortOrder(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            _ => 6
        };
    }

    public async Task<bool> Delete(int id)
    {
        _permissionService.Ensure(PermissionKeys.WORK_POLICY_DELETE);
        return await _dal.Delete(id);
    }
}
     