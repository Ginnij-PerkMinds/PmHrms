using Hangfire;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class TaskBAL : ITaskBAL
    {
        private readonly TaskDAL _taskDAL;
        private readonly IPermissionService _permissionService;
        private readonly ITenantService _currentUser;
        private readonly IBackgroundJobClient _jobClient;

        public TaskBAL(
            TaskDAL taskDAL,
            IPermissionService permissionService,
            ITenantService currentUser,
            IBackgroundJobClient jobClient)
        {
            _taskDAL           = taskDAL;
            _permissionService = permissionService;
            _currentUser       = currentUser;
            _jobClient         = jobClient;
        }

        

        public async Task<(List<TaskResponseModel> Items, int TotalCount)> GetAllTasks(
            int pageNumber, int pageSize,
            string? searchTerm, string? status, int? priority)
        {
            _permissionService.Ensure(PermissionKeys.TASK_VIEW);

            int OrgId  = _currentUser.GetOrgId();

            var (entities, total) = await _taskDAL.GetAllTasks(
                OrgId, pageNumber, pageSize, searchTerm, status, priority);

            var items = new List<TaskResponseModel>();
            foreach (var e in entities)
            {

                var (totalAsgn, totalDone) = await _taskDAL.GetProgressCounts(e.Id);
                var model = MapToResponse(e, totalAsgn, totalDone);
                items.Add(model);
            }

            return (items, total);
        }

        public async Task<TaskResponseModel?> GetTask(int taskId)
        {
            _permissionService.Ensure(PermissionKeys.TASK_VIEW);

                 int OrgId  = _currentUser.GetOrgId();

            var entity = await _taskDAL.GetTask(taskId, OrgId);
            if (entity == null) return null;

            var (totalAsgn, totalDone) = await _taskDAL.GetProgressCounts(taskId);
            return MapToResponse(entity, totalAsgn, totalDone);
        }

        public async Task<TaskResponseModel?> AddTask(TaskModel model)
        {
            _permissionService.Ensure(PermissionKeys.TASK_CREATE);

                 int OrgId  = _currentUser.GetOrgId();
                 int EmpId = _currentUser.GetCurrentUserID();

            var entity = new TaskEntity
            {
                OrgId            = OrgId,
                PostId           = model.PostId,
                Title            = model.Title,
                Description      = model.Description,
                Priority         = (byte)model.Priority,
                AssignedByUserId =  EmpId,
                Status           = TaskStatuses.Pending,
                DueDate          = model.DueDate,
                 ReviewerType       = model.ReviewerType,

                //ReviewerEmployeeId = model.ReviewerType == "Employee"
                //               ? model.ReviewerEmployeeId
                //               : null,
                ReviewerEmployeeId = model.ReviewerType == PmHrmsConstants.TaskMessages.Employee
                                       ? model.ReviewerEmployeeId
                                       : null,

                IsDeleted        = false,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow,

               
                Assignments = model.Assignments.Select(a => new TaskAssignment
                {
                    OrgId      = OrgId,
                    TargetType = a.TargetType,
                    TargetId   = a.TargetId
                }).ToList()
            };

            var created = await _taskDAL.AddTask(entity);

            
            _jobClient.Enqueue<TaskBAL>(bal =>
                bal.ExpandAssignmentsAsync(created.Id, OrgId));

            return await GetTask(created.Id);
        }

        public async Task<TaskResponseModel?> UpdateTask(int taskId, TaskModel model)
        {
            _permissionService.Ensure(PermissionKeys.TASK_EDIT);

                int OrgId  = _currentUser.GetOrgId();
                 int EmpId = _currentUser.GetCurrentUserID();
            var entity = new TaskEntity
            {
                
                Id          = taskId,
                OrgId       = OrgId,
                PostId      = model.PostId,
                Title       = model.Title,
                Description = model.Description,
                Priority    = (byte)model.Priority,
                DueDate     = model.DueDate,
                UpdatedAt   = DateTime.UtcNow,
                 ReviewerType       = model.ReviewerType,
                 ReviewerEmployeeId = model.ReviewerType == "Employee"
                                ? model.ReviewerEmployeeId
                                : null,
               
                Assignments = model.Assignments.Select(a => new TaskAssignment
                {
                    OrgId      = OrgId,
                    TaskId     = taskId,
                    TargetType = a.TargetType,
                    TargetId   = a.TargetId
                }).ToList()
            };

            var result = await _taskDAL.UpdateTask(entity);
            if (result == null) return null;

            _jobClient.Enqueue<TaskBAL>(bal =>
                bal.ExpandAssignmentsAsync(taskId, OrgId));

            return await GetTask(taskId);
        }

        public async Task<bool> DeleteTask(int taskId)
        {
            _permissionService.Ensure(PermissionKeys.TASK_DELETE);
              int OrgId  = _currentUser.GetOrgId();
                 int EmpId = _currentUser.GetCurrentUserID();
            return await _taskDAL.DeleteTask(taskId, OrgId);
        }

        // ════════════════════════════════════════════════════
        // HANGFIRE JOB: Expand assignments → progress rows
        // ════════════════════════════════════════════════════

        [AutomaticRetry(Attempts = 3)]
        public async Task ExpandAssignmentsAsync(int taskId, int orgId)
        {
            var task = await _taskDAL.GetTask(taskId, orgId);
            if (task == null) return;

            var existingIds = await _taskDAL.GetExistingProgressEmployeeIds(taskId);
            var targetIds   = new HashSet<int>();

            
            foreach (var assignment in task.Assignments)
            {
                IEnumerable<int> empIds = assignment.TargetType switch
                {
                    //"Employee"    => assignment.TargetId.HasValue
                    //                    ? new[] { assignment.TargetId.Value }
                    //                    : Array.Empty<int>(),
                    PmHrmsConstants.TaskMessages.Employee => assignment.TargetId.HasValue
                                            ? new[] { assignment.TargetId.Value }
                                            : Array.Empty<int>(),

                    //"Department"  => assignment.TargetId.HasValue
                    //                    ? await _taskDAL.GetEmployeeIdsByDepartment(assignment.TargetId.Value, orgId)
                    //                    : Array.Empty<int>(),
                    PmHrmsConstants.TaskMessages.Department => assignment.TargetId.HasValue
                                            ? await _taskDAL.GetEmployeeIdsByDepartment(assignment.TargetId.Value, orgId)
                                            : Array.Empty<int>(),

                    //"Designation" => assignment.TargetId.HasValue
                    //                    ? await _taskDAL.GetEmployeeIdsByDesignation(assignment.TargetId.Value, orgId)
                    //                    : Array.Empty<int>(),
                    PmHrmsConstants.TaskMessages.Designation => assignment.TargetId.HasValue
                                            ? await _taskDAL.GetEmployeeIdsByDesignation(assignment.TargetId.Value, orgId)
                                            : Array.Empty<int>(),

                    //"All"         => await _taskDAL.GetAllEmployeeIds(orgId),
                    PmHrmsConstants.TaskMessages.All => await _taskDAL.GetAllEmployeeIds(orgId),

                    //_ => Array.Empty<int>()
                    PmHrmsConstants.TaskMessages.Direct => Array.Empty<int>()
                };

                foreach (var id in empIds)
                    targetIds.Add(id);
            }

            var newRows = targetIds
                .Where(id => !existingIds.Contains(id))
                .Select(empId =>
                {

                    //var sourceAssignment = task.Assignments.FirstOrDefault(a =>
                    //    a.TargetType == "All" ||
                    //    (a.TargetType == "Employee" && a.TargetId == empId));
                    var sourceAssignment = task.Assignments.FirstOrDefault(a =>
                        a.TargetType == PmHrmsConstants.TaskMessages.All ||
                        (a.TargetType == PmHrmsConstants.TaskMessages.Employee && a.TargetId == empId));
                       

                    return new TaskEmployeeProgress
                    {
                        TaskId     = taskId,
                        OrgId      = orgId,
                        EmployeeId = empId,
                        Status     = TaskStatuses.Pending,

                        //SourceType = sourceAssignment?.TargetType ?? "Direct",
                        SourceType = sourceAssignment?.TargetType ?? PmHrmsConstants.TaskMessages.Direct,

                        SourceId   = sourceAssignment?.TargetId,
                        CreatedAt  = DateTime.UtcNow,
                        UpdatedAt  = DateTime.UtcNow
                    };
                })
                .ToList();

            if (newRows.Any())
                await _taskDAL.BulkInsertProgress(newRows);

        //if (task.ReviewerType == "DepartmentHead" && task.ReviewerEmployeeId == null)
        if (task.ReviewerType == PmHrmsConstants.TaskMessages.DepartmentHead && task.ReviewerEmployeeId == null)

            {
                var headId = await _taskDAL.GetDepartmentHeadEmployeeId(taskId, orgId);
                if (headId.HasValue)
                    await _taskDAL.UpdateReviewerEmployee(taskId, orgId, headId.Value);
            }
        }
        // ════════════════════════════════════════════════════
        // PROGRESS TRACKING
        // ════════════════════════════════════════════════════

        public async Task<(List<TaskProgressResponseModel> Items, int TotalCount)> GetTaskProgress(
            int taskId, int pageNumber, int pageSize, string? status)
        {
            _permissionService.Ensure(PermissionKeys.TASK_VIEW);
              int OrgId  = _currentUser.GetOrgId();
                 int EmpId = _currentUser.GetCurrentUserID();

            var (entities, total) = await _taskDAL.GetTaskProgress(
                taskId, OrgId, pageNumber, pageSize, status);

            var items = entities.Select(p => new TaskProgressResponseModel
            {
                
                ProgressId   = p.Id,
                TaskId       = p.TaskId,
                TaskTitle    = p.Task?.Title ?? string.Empty,
                EmployeeId   = p.EmployeeId,
                EmployeeName = p.Employee != null
                    ? $"{p.Employee.FirstName} {p.Employee.LastName}".Trim()
                    : string.Empty,
                Status      = p.Status,
                CompletedAt = p.CompletedAt,
                Remarks     = p.Remarks,
                SourceType  = p.SourceType,
                SourceId    = p.SourceId,

                //ReviewerType = p.Task?.ReviewerType ?? "Self",
                ReviewerType = p.Task?.ReviewerType ?? PmHrmsConstants.TaskMessages.Self,

                ReviewerEmployeeId = p.Task?.ReviewerEmployeeId,
                ReviewerName = p.Task?.ReviewerEmployee != null
                    ? $"{p.Task.ReviewerEmployee.FirstName} {p.Task.ReviewerEmployee.LastName}".Trim()
                    : null
            }).ToList();

            return (items, total);                     
        }

        public async Task<bool> UpdateMyTaskStatus(int taskId, int employeeId, UpdateTaskStatusModel model)           
        {
            int OrgId = _currentUser.GetOrgId();        
            var progress = await _taskDAL.GetMyProgress(taskId, employeeId);       
            if (progress == null || progress.Task == null || progress.Task.IsDeleted) return false;       

            if (progress.Status == TaskStatuses.UnderReview || progress.Status == TaskStatuses.Completed)
            {     
                return false;
            }

            //var isSelfReview = string.Equals(progress.Task.ReviewerType, "Self", StringComparison.OrdinalIgnoreCase);
            var isSelfReview = string.Equals(progress.Task.ReviewerType, PmHrmsConstants.TaskMessages.Self, StringComparison.OrdinalIgnoreCase);

            var trimmedRemarks = string.IsNullOrWhiteSpace(model.Remarks) ? null : model.Remarks.Trim();

            if (model.Status == TaskStatuses.Completed)
            {
                if (!isSelfReview)       
                {   
                    return false;  
                }
            }
            else if (model.Status == TaskStatuses.ReviewRequested && isSelfReview)
            {
                return false;     
            }

            progress.Status = model.Status;  
            progress.Remarks = trimmedRemarks;
            progress.CompletedAt = model.Status == TaskStatuses.Completed ? DateTime.UtcNow : null;
            progress.UpdatedAt = DateTime.UtcNow;

            var updated = await _taskDAL.UpdateProgress(progress);
            if (!updated)
            {
                return false;
            }

            var counts = await _taskDAL.GetTaskStatusCounts(taskId, OrgId);
            var aggregateStatus = ResolveAggregateStatus(counts);
            DateTime? aggregateCompletedAt = aggregateStatus == TaskStatuses.Completed ? DateTime.UtcNow : null;

            await _taskDAL.UpdateTaskAggregateStatus(taskId, OrgId, aggregateStatus, aggregateCompletedAt);

            return true;
        }

        public async Task<bool> UpdateTaskReviewStatus(int progressId, int reviewerEmployeeId, UpdateTaskReviewModel model)
        {
            int orgId = _currentUser.GetOrgId();
            var progress = await _taskDAL.GetProgressForReview(progressId, reviewerEmployeeId, orgId);
            if (progress == null)
            {
                return false;
            }

            if (model.Status == TaskStatuses.UnderReview && progress.Status != TaskStatuses.ReviewRequested)
            {
                return false;
            }

            if (model.Status == TaskStatuses.Completed && progress.Status != TaskStatuses.UnderReview)
            {
                return false;
            }

            DateTime? completedAt = model.Status == TaskStatuses.Completed ? DateTime.UtcNow : null;
            var trimmedRemarks = string.IsNullOrWhiteSpace(model.Remarks) ? null : model.Remarks.Trim();

            progress.Status = model.Status;
            progress.CompletedAt = completedAt;
            progress.ReviewRemarks = trimmedRemarks;
            progress.ReviewedAt = DateTime.UtcNow;
            progress.ReviewedByEmployeeId = reviewerEmployeeId;
            progress.UpdatedAt = DateTime.UtcNow;

            var updated = await _taskDAL.UpdateProgress(progress);
            if (!updated)
            {
                return false;
            }

            var counts = await _taskDAL.GetTaskStatusCounts(progress.TaskId, orgId);
            var aggregateStatus = ResolveAggregateStatus(counts);
            var aggregateCompletedAt = aggregateStatus == TaskStatuses.Completed ? completedAt : null;

            await _taskDAL.UpdateTaskReviewState(
                progress.TaskId,
                orgId,
                aggregateStatus,
                aggregateCompletedAt,
                trimmedRemarks,
                progress.ReviewedAt);

            return true;
        }

        // ════════════════════════════════════════════════════
        // FOLLOW-UPS
        // ════════════════════════════════════════════════════

        public async Task<List<TaskFollowUpResponseModel>> GetFollowUps(
            int taskId,
            bool employeeContext = false,
            bool markAsRead = false)
        {
            _permissionService.Ensure(PermissionKeys.TASK_VIEW);
            int OrgId = _currentUser.GetOrgId();
            int EmpId = _currentUser.GetCurrentUserID();

            var entities = await _taskDAL.GetFollowUps(taskId, OrgId);
            TaskEmployeeProgress? myProgress = null;

            if (employeeContext)
            {
                myProgress = await _taskDAL.GetMyProgress(taskId, EmpId);
                if (myProgress == null)
                {
                    return new List<TaskFollowUpResponseModel>();
                }

                entities = entities
                    .Where(f => IsFollowUpVisibleToEmployee(f, myProgress.Status, EmpId))
                    .ToList();

                if (markAsRead && entities.Count > 0)
                {
                    var readAt = DateTime.UtcNow;
                    var followUpIds = entities.Select(f => f.Id).ToList();

                    await _taskDAL.MarkFollowUpReceiptsRead(followUpIds, EmpId, OrgId, readAt);

                    foreach (var receipt in entities
                                 .SelectMany(f => f.Receipts)
                                 .Where(r => r.EmployeeId == EmpId && !r.IsRead))
                    {
                        receipt.IsRead = true;
                        receipt.ReadAt = readAt;
                    }
                }
            }

            return entities.Select(f => new TaskFollowUpResponseModel
            {
                FollowUpId = f.Id,
                TaskId = f.TaskId,
                Message = f.Message,
                CreatedByName = f.CreatedByEmployee != null
                    ? $"{f.CreatedByEmployee.FirstName} {f.CreatedByEmployee.LastName}".Trim()
                    : string.Empty,
                TargetType = f.TargetType,
                IsScheduled = f.IsScheduled,
                ScheduledAt = f.ScheduledAt,
                IsSent = f.IsSent,
                SentAt = f.SentAt,
                CreatedAt = f.CreatedAt,
                TotalReceipts = f.Receipts.Count,
                ReadCount = f.Receipts.Count(r => r.IsRead),
                IsRead = employeeContext
                    ? f.Receipts.FirstOrDefault(r => r.EmployeeId == EmpId)?.IsRead
                    : null,
                ReadAt = employeeContext
                    ? f.Receipts.FirstOrDefault(r => r.EmployeeId == EmpId)?.ReadAt
                    : null
            }).ToList();
        }

        public async Task<TaskFollowUpResponseModel?> AddFollowUp(int taskId, TaskFollowUpModel model)
        {
            _permissionService.Ensure(PermissionKeys.TASK_CREATE);
              int OrgId  = _currentUser.GetOrgId();
                 int EmpId = _currentUser.GetCurrentUserID();

            var followUp = new TaskFollowUp
            {
                TaskId          = taskId,
                OrgId           = OrgId,
                Message         = model.Message,
                CreatedByUserId = EmpId,
                TargetType      = model.TargetType,
                IsScheduled     = model.IsScheduled,
                ScheduledAt     = model.ScheduledAt?.ToUniversalTime(),
                IsSent          = false,
                CreatedAt       = DateTime.UtcNow
            };

            var created = await _taskDAL.AddFollowUp(followUp);

            if (!model.IsScheduled)
            {
                
                _jobClient.Enqueue<TaskBAL>(bal =>
                    bal.SendFollowUpAsync(created.Id, OrgId));
            }
            else if (model.ScheduledAt.HasValue)
            {
                _jobClient.Schedule<TaskBAL>(
                    bal => bal.SendFollowUpAsync(created.Id,OrgId),
                    model.ScheduledAt.Value.ToUniversalTime());
            }

            var followUps = await GetFollowUps(taskId);
            return followUps.FirstOrDefault(f => f.FollowUpId == created.Id);
        }

        public async Task<List<TaskNoteResponseModel>> GetNotes(int taskId)
            {
                _permissionService.Ensure(PermissionKeys.TASK_VIEW);
                int orgId = _currentUser.GetOrgId();

                var entities = await _taskDAL.GetNotes(taskId, orgId);

                return entities.Select(n => new TaskNoteResponseModel
                {
                    NoteId        = n.Id,
                    TaskId        = n.TaskId,
                    Content       = n.Content,
                    CreatedByUserId = n.CreatedByUserId,
                    CreatedByName = n.CreatedByEmployee != null
                        ? $"{n.CreatedByEmployee.FirstName} {n.CreatedByEmployee.LastName}".Trim()
                        : null,
                    MentionedEmployeeId   = n.MentionedEmployeeId,
                    MentionedEmployeeName = n.MentionedEmployee != null
                        ? $"{n.MentionedEmployee.FirstName} {n.MentionedEmployee.LastName}".Trim()
                        : null,
                    CreatedAt = n.CreatedAt
                }).ToList();
            }

            public async Task<TaskNoteResponseModel?> AddNote(
                int taskId, int createdByEmployeeId, TaskNoteModel model)
            {
                _permissionService.Ensure(PermissionKeys.TASK_VIEW); 
                int orgId = _currentUser.GetOrgId();

                var note = new TaskNote
                {
                    TaskId               = taskId,
                    OrgId                = orgId,
                    Content              = model.Content.Trim(),
                    CreatedByUserId      = createdByEmployeeId,
                    MentionedEmployeeId  = model.MentionedEmployeeId,
                    CreatedAt            = DateTime.UtcNow
                };

                var created = await _taskDAL.AddNote(note);

                
                var notes = await GetNotes(taskId);
                return notes.FirstOrDefault(n => n.NoteId == created.Id);
            }

            public async Task<List<MyTaskResponseModel>> GetMyTasks(
    int employeeId, string? status, int? priority)
{
    int orgId = _currentUser.GetOrgId();

    var rows = await _taskDAL.GetMyTasks(employeeId, orgId, status, priority);

    return rows.Select(p => new MyTaskResponseModel
    {
        ProgressId    = p.Id,
        TaskId        = p.TaskId,
        Title         = p.Task?.Title ?? string.Empty,
        Description   = p.Task?.Description,
        Priority      = p.Task?.Priority ?? 0,
        PriorityLabel = PriorityLabel(p.Task?.Priority ?? 0),
        PostTitle     = p.Task?.Post?.Title,
        Status        = p.Status,
        DueDate       = p.Task?.DueDate,
        CompletedAt   = p.CompletedAt,
        Remarks       = p.Remarks,

        //ReviewerType  = p.Task?.ReviewerType ?? "Self",
        ReviewerType = p.Task?.ReviewerType ?? PmHrmsConstants.TaskMessages.Self,

        ReviewerName  = p.Task?.ReviewerEmployee != null
            ? $"{p.Task.ReviewerEmployee.FirstName} {p.Task.ReviewerEmployee.LastName}".Trim()
            : null,
        CreatedAt = p.CreatedAt
    }).ToList();
}

// ════════════════════════════════════════════════════
// REVIEWING TASKS
// ════════════════════════════════════════════════════

public async Task<List<ReviewingTaskResponseModel>> GetReviewingTasks(int employeeId)
{
    int orgId = _currentUser.GetOrgId();

    var progressRows = await _taskDAL.GetReviewingTasks(employeeId, orgId);

    var result = new List<ReviewingTaskResponseModel>();
    var progressSummaryCache = new Dictionary<int, (int Total, int Completed)>();
    var statusCache = new Dictionary<int, (int Total, int Completed, int InProgress, int ReviewRequested, int UnderReview)>();

    foreach (var progress in progressRows)
    {
        if (!progressSummaryCache.TryGetValue(progress.TaskId, out var progressSummary))
        {
            progressSummary = await _taskDAL.GetProgressCounts(progress.TaskId);
            progressSummaryCache[progress.TaskId] = progressSummary;
        }

        if (!statusCache.TryGetValue(progress.TaskId, out var statusCounts))
        {
            statusCounts = await _taskDAL.GetTaskStatusCounts(progress.TaskId, orgId);
            statusCache[progress.TaskId] = statusCounts;
        }

        var overallTaskStatus = ResolveAggregateStatus(statusCounts);

        result.Add(new ReviewingTaskResponseModel
        {
            ProgressId     = progress.Id,
            TaskId         = progress.TaskId,
            EmployeeId     = progress.EmployeeId,
            EmployeeName   = progress.Employee != null
                ? $"{progress.Employee.FirstName} {progress.Employee.LastName}".Trim()
                : string.Empty,
            Title          = progress.Task?.Title ?? string.Empty,
            Description    = progress.Task?.Description,
            Priority       = progress.Task?.Priority ?? 0,
            PriorityLabel  = PriorityLabel(progress.Task?.Priority ?? 0),
            PostTitle      = progress.Task?.Post?.Title,
            OverallStatus  = progress.Status,
            OverallTaskStatus = overallTaskStatus,
            DueDate        = progress.Task?.DueDate,
            AssignedByName = progress.Task?.AssignedByEmployee != null
                ? $"{progress.Task.AssignedByEmployee.FirstName} {progress.Task.AssignedByEmployee.LastName}".Trim()
                : string.Empty,
            TotalAssigned   = progressSummary.Total,
            TotalCompleted  = progressSummary.Completed,
            ProgressPercent = progressSummary.Total > 0
                ? (int)Math.Round((double)progressSummary.Completed / progressSummary.Total * 100)
                : 0,
            EmployeeRemarks = progress.Remarks,
            StatusUpdatedAt = progress.UpdatedAt,
            ReviewRemarks = progress.ReviewRemarks,
            ReviewedAt    = progress.ReviewedAt,
            ReviewedByName = progress.ReviewedByEmployee != null
                ? $"{progress.ReviewedByEmployee.FirstName} {progress.ReviewedByEmployee.LastName}".Trim()
                : null,
            CreatedAt     = progress.CreatedAt
        });
    }

    return result;
}



        // ── HANGFIRE JOB: Send follow-up ─────────────────────────────
        [AutomaticRetry(Attempts = 3)]
        public async Task SendFollowUpAsync(int followUpId, int orgId)
        {
            var followUp = await _taskDAL.GetFollowUpById(followUpId, orgId);
            if (followUp == null || followUp.IsSent) return;

            List<int> targetEmpIds = followUp.TargetType switch
            {
                //"All"     => await _taskDAL.GetAllEmployeeIds(orgId),
                PmHrmsConstants.TaskMessages.All => await _taskDAL.GetAllEmployeeIds(orgId),

                TaskStatuses.Pending => await _taskDAL.GetPendingEmployeeIds(followUp.TaskId),
                _         => new List<int>()
            };

            var receipts = targetEmpIds.Select(empId => new TaskFollowUpReceipt
            {
                FollowUpId = followUpId,
                OrgId      = orgId,
                EmployeeId = empId,
                IsRead     = false
            }).ToList();

            if (receipts.Any())
                await _taskDAL.BulkInsertReceipts(receipts);

            await _taskDAL.MarkFollowUpSent(followUpId);

            
        }

        private static bool IsFollowUpVisibleToEmployee(
            TaskFollowUp followUp,
            string progressStatus,
            int employeeId)
        {
            var receipt = followUp.Receipts.FirstOrDefault(r => r.EmployeeId == employeeId);
            if (receipt == null)
            {
                return false;
            }

            if (!followUp.IsSent)
            {
                return false;
            }

            if (followUp.TargetType == TaskStatuses.Pending)
            {
                return progressStatus == TaskStatuses.Pending
                    || progressStatus == TaskStatuses.InProgress;
            }

            return true;
        }

        // ── MAPPER ───────────────────────────────────────────────────
        private static string PriorityLabel(int p) => p switch
        {
            //1 => "Low",
            1 => PmHrmsConstants.TaskMessages.Low,

            //2 => "Medium",
            2 => PmHrmsConstants.TaskMessages.Medium,

            //3 => "High",
            3 => PmHrmsConstants.TaskMessages.High,

            //4 => "Critical",
            4 => PmHrmsConstants.TaskMessages.Critical,

            //_ => "Unknown"
            _ => PmHrmsConstants.TaskMessages.Unknown
        };

        private static string ResolveAggregateStatus(
            (int Total, int Completed, int InProgress, int ReviewRequested, int UnderReview) counts)
        {
            if (counts.Total == 0)
            {
                return TaskStatuses.Pending;
            }

            if (counts.Completed == counts.Total)
            {
                return TaskStatuses.Completed;
            }

            if (counts.UnderReview > 0 && counts.Completed + counts.UnderReview == counts.Total)
            {
                return TaskStatuses.UnderReview;
            }

            if (counts.ReviewRequested > 0 && counts.Completed + counts.ReviewRequested == counts.Total)
            {
                return TaskStatuses.ReviewRequested;
            }

            if (counts.InProgress > 0 || counts.ReviewRequested > 0 || counts.UnderReview > 0 || counts.Completed > 0)
            {
                return TaskStatuses.InProgress;
            }

            return TaskStatuses.Pending;
        }

        private static TaskResponseModel MapToResponse(TaskEntity e, int totalAsgn, int totalDone) => new()
        {
            
            TaskId        = e.Id,
            OrgId         = e.OrgId,
            PostId        = e.PostId,
            PostTitle     = e.Post?.Title,
            Title         = e.Title,
            Description   = e.Description,
            Priority      = e.Priority,
            PriorityLabel = PriorityLabel(e.Priority),
            Status        = e.Status,

            DueDate         = e.DueDate ?? DateTime.MinValue,
            CompletedAt   = e.CompletedAt,

            AssignedByName = e.AssignedByEmployee != null
                ? $"{e.AssignedByEmployee.FirstName} {e.AssignedByEmployee.LastName}".Trim()

                    //: "System",
                    : PmHrmsConstants.TaskMessages.SystemAssigned,

            ReviewerType       = e.ReviewerType,
            ReviewerEmployeeId = e.ReviewerEmployeeId,
            ReviewerName = e.ReviewerEmployee != null
                ? $"{e.ReviewerEmployee.FirstName} {e.ReviewerEmployee.LastName}".Trim()
                : (e.ReviewerType == "Self" ? "Self Review" : "Pending Assignment"),
            ReviewRemarks = e.ReviewRemarks,
            ReviewedAt    = e.ReviewedAt,   
            CreatedAt       = e.CreatedAt,
            UpdatedAt       = e.UpdatedAt,
            TotalAssigned   = totalAsgn,
            TotalCompleted  = totalDone,
            ProgressPercent = totalAsgn > 0
                ? (int)Math.Round((double)totalDone / totalAsgn * 100)
                : 0,


            Assignments = e.Assignments.Select(a => new TaskAssignmentResponseModel
            {
               
                AssignmentId = a.Id,
                TargetType   = a.TargetType,
                TargetId     = a.TargetId,
                TargetName   = null
            }).ToList()
        };
    }
}
