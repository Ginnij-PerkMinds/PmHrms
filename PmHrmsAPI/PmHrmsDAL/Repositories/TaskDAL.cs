using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class TaskDAL
    {
        private readonly PmHrmsContext _context;

        public TaskDAL(PmHrmsContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════
        // TASK CRUD
        // ════════════════════════════════════════════════════

        public async Task<(List<TaskEntity> Items, int TotalCount)> GetAllTasks(
            int orgId, int pageNumber, int pageSize,
            string? searchTerm, string? status, int? priority)
        {
            var query = _context.Tasks
                .Where(t => t.OrgId == orgId && !t.IsDeleted)
                .Include(t => t.AssignedByEmployee)
                .Include(t => t.ReviewerEmployee)
                .Include(t => t.Post)
                .Include(t => t.Assignments)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            int totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<TaskEntity?> GetTask(int taskId, int orgId)
        {
            return await _context.Tasks
                .IgnoreQueryFilters()     
                .Where(t => t.Id == taskId && t.OrgId == orgId && !t.IsDeleted)
                .Include(t => t.AssignedByEmployee)
                .Include(t => t.ReviewerEmployee)  
                .Include(t => t.Post)
                .Include(t => t.Assignments)
                .Include(t => t.FollowUps)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<TaskEntity> AddTask(TaskEntity task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskEntity?> UpdateTask(TaskEntity task)
        {
            var existing = await _context.Tasks
                .Include(t => t.Assignments)     
                .FirstOrDefaultAsync(t =>        
                    t.Id == task.Id &&           
                    t.OrgId == task.OrgId &&
                    !t.IsDeleted);

            if (existing == null) return null;

            existing.Title       = task.Title;
            existing.Description = task.Description;
            existing.Priority    = task.Priority;
            existing.DueDate     = task.DueDate;
            existing.PostId      = task.PostId;
            existing.ReviewerType       = task.ReviewerType;
            existing.ReviewerEmployeeId = task.ReviewerEmployeeId;
            existing.UpdatedAt          = DateTime.UtcNow;

            
            _context.TaskAssignments.RemoveRange(existing.Assignments);
            existing.Assignments = task.Assignments;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTask(int taskId, int orgId)
        {
            var existing = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.OrgId == orgId && !t.IsDeleted);

            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

       

        public async Task<(List<TaskEmployeeProgress> Items, int TotalCount)> GetTaskProgress(
            int taskId, int orgId, int pageNumber, int pageSize, string? status)
        {
            var query = _context.TaskEmployeeProgresses
                .Where(p => p.TaskId == taskId && p.OrgId == orgId)
                .Include(p => p.Task)
                    .ThenInclude(t => t!.ReviewerEmployee)
                .Include(p => p.Employee)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            int total = await query.CountAsync();

            var data = await query
                .OrderBy(p => p.Status)
                .ThenBy(p => p.EmployeeId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<TaskEmployeeProgress?> GetMyProgress(int taskId, int employeeId)
        {
            return await _context.TaskEmployeeProgresses
                .Include(p => p.Task)
                .FirstOrDefaultAsync(p => p.TaskId == taskId && p.EmployeeId == employeeId);
        }

        public async Task<TaskEmployeeProgress?> GetProgressForReview(int progressId, int reviewerEmployeeId, int orgId)
        {
            return await _context.TaskEmployeeProgresses
                .Include(p => p.Task)
                .Include(p => p.Employee)
                .Include(p => p.ReviewedByEmployee)
                .FirstOrDefaultAsync(p =>
                    p.Id == progressId
                    && p.OrgId == orgId
                    && p.Task.ReviewerEmployeeId == reviewerEmployeeId
                    && !p.Task.IsDeleted);
        }

        public async Task<bool> UpdateProgress(TaskEmployeeProgress progress)
        {
            _context.TaskEmployeeProgresses.Update(progress);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateProgressStatusesForReview(
            int taskId,
            int orgId,
            IReadOnlyCollection<string> fromStatuses,
            string nextStatus,
            DateTime? completedAt)
        {
            var rows = await _context.TaskEmployeeProgresses
                .Where(p => p.TaskId == taskId && p.OrgId == orgId && fromStatuses.Contains(p.Status))
                .ToListAsync();

            if (!rows.Any())
            {
                return 0;
            }

            var updatedAt = DateTime.UtcNow;
            foreach (var row in rows)
            {
                row.Status = nextStatus;
                row.CompletedAt = completedAt;
                row.UpdatedAt = updatedAt;
            }

            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<(int Total, int Completed)> GetProgressCounts(int taskId)
        {
            var counts = await _context.TaskEmployeeProgresses
                .AsNoTracking()
                .Where(p => p.TaskId == taskId)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    Total     = g.Count(),
                    Completed = g.Count(p => p.Status == TaskStatuses.Completed)
                })
                .FirstOrDefaultAsync();                                                                                  

            return counts == null ? (0, 0) : (counts.Total, counts.Completed);
        }                       
                    
        public async Task BulkInsertProgress(List<TaskEmployeeProgress> rows)
        {
            await _context.TaskEmployeeProgresses.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetEmployeeIdsByDepartment(int deptId, int orgId)
        {
            return await _context.Employees
                .IgnoreQueryFilters()         
                .AsNoTracking()
                .Where(e => e.DepartmentId == deptId && e.OrganizationId == orgId && e.IsActive)
                .Select(e => e.EmployeeId)
                .ToListAsync();
        }

        public async Task<List<int>> GetEmployeeIdsByDesignation(int desigId, int orgId)
        {
            return await _context.Employees
                .IgnoreQueryFilters()                                     
                .AsNoTracking()                       
                .Where(e => e.DesignationId == desigId && e.OrganizationId == orgId && e.IsActive)
                .Select(e => e.EmployeeId)                               
                .ToListAsync();
        }

        public async Task<List<int>> GetAllEmployeeIds(int orgId)
        {
            return await _context.Employees
                .IgnoreQueryFilters()  
                .AsNoTracking()
                .Where(e => e.OrganizationId == orgId && e.IsActive)
                .Select(e => e.EmployeeId)
                .ToListAsync();
        }

        public async Task<HashSet<int>> GetExistingProgressEmployeeIds(int taskId)
        {
            var ids = await _context.TaskEmployeeProgresses
                .AsNoTracking()
                .Where(p => p.TaskId == taskId)
                .Select(p => p.EmployeeId)
                .ToListAsync();

            return new HashSet<int>(ids);
        }

        // ════════════════════════════════════════════════════
        // FOLLOW-UPS
        // ════════════════════════════════════════════════════

        public async Task<List<TaskFollowUp>> GetFollowUps(int taskId, int orgId)
        {
            return await _context.TaskFollowUps
                .Where(f => f.TaskId == taskId && f.OrgId == orgId)
                .Include(f => f.Receipts)
                .Include(f => f.CreatedByEmployee)
                .Include(f => f.Task)
                .OrderByDescending(f => f.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> MarkFollowUpReceiptsRead(
            IReadOnlyCollection<int> followUpIds,
            int employeeId,
            int orgId,
            DateTime readAt)
        {
            if (followUpIds.Count == 0)
            {
                return 0;
            }

            var receipts = await _context.TaskFollowUpReceipts
                .Where(r =>
                    r.OrgId == orgId
                    && r.EmployeeId == employeeId
                    && followUpIds.Contains(r.FollowUpId)
                    && !r.IsRead)
                .ToListAsync();

            if (!receipts.Any())
            {
                return 0;
            }

            foreach (var receipt in receipts)
            {
                receipt.IsRead = true;
                receipt.ReadAt = readAt;
            }

            await _context.SaveChangesAsync();
            return receipts.Count;
        }

        public async Task<TaskFollowUp> AddFollowUp(TaskFollowUp followUp)
        {
            await _context.TaskFollowUps.AddAsync(followUp);
            await _context.SaveChangesAsync();
            return followUp;
        }

        public async Task<TaskFollowUp?> GetFollowUpById(int followUpId, int orgId)
        {
            return await _context.TaskFollowUps
                .FirstOrDefaultAsync(f => f.Id == followUpId && f.OrgId == orgId); 
        }

        public async Task BulkInsertReceipts(List<TaskFollowUpReceipt> receipts)
        {
            await _context.TaskFollowUpReceipts.AddRangeAsync(receipts);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetPendingEmployeeIds(int taskId)
        {
            return await _context.TaskEmployeeProgresses
                .AsNoTracking()
                .Where(p =>
                    p.TaskId == taskId
                    && p.Status != TaskStatuses.ReviewRequested
                    && p.Status != TaskStatuses.UnderReview
                    && p.Status != TaskStatuses.Completed)
                .Select(p => p.EmployeeId)
                .ToListAsync();
        }

        public async Task MarkFollowUpSent(int followUpId)
        {
            var followUp = await _context.TaskFollowUps.FindAsync(followUpId);
            if (followUp == null) return;

            followUp.IsSent = true;
            followUp.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<(int Total, int Completed, int InProgress, int ReviewRequested, int UnderReview)> GetTaskStatusCounts(int taskId, int orgId)
        {
            var counts = await _context.TaskEmployeeProgresses
                .AsNoTracking()
                .Where(p => p.TaskId == taskId && p.OrgId == orgId)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(p => p.Status == TaskStatuses.Completed),
                    InProgress = g.Count(p => p.Status == TaskStatuses.InProgress),
                    ReviewRequested = g.Count(p => p.Status == TaskStatuses.ReviewRequested),
                    UnderReview = g.Count(p => p.Status == TaskStatuses.UnderReview)
                })
                .FirstOrDefaultAsync();

            return counts == null
                ? (0, 0, 0, 0, 0)
                : (counts.Total, counts.Completed, counts.InProgress, counts.ReviewRequested, counts.UnderReview);
        }

        public async Task UpdateTaskAggregateStatus(int taskId, int orgId, string status, DateTime? completedAt)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.OrgId == orgId && !t.IsDeleted);

            if (task == null)
            {
                return;
            }

            task.Status = status;
            task.CompletedAt = completedAt;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskReviewState(
            int taskId,
            int orgId,
            string status,
            DateTime? completedAt,
            string? reviewRemarks,
            DateTime? reviewedAt)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.OrgId == orgId && !t.IsDeleted);

            if (task == null)
            {
                return;
            }

            task.Status = status;
            task.CompletedAt = completedAt;
            task.ReviewRemarks = reviewRemarks;
            task.ReviewedAt = reviewedAt;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskNote>> GetNotes(int taskId, int orgId)
        {
            return await _context.TaskNotes
                .Where(n => n.TaskId == taskId && n.OrgId == orgId)
                .Include(n => n.CreatedByEmployee)
                .Include(n => n.MentionedEmployee)
                .OrderByDescending(n => n.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TaskNote> AddNote(TaskNote note)
        {
            await _context.TaskNotes.AddAsync(note);
            await _context.SaveChangesAsync();
            return note;
        }



        public async Task<List<TaskEmployeeProgress>> GetMyTasks(
    int employeeId, int orgId, string? status, int? priority)
            {
                var query = _context.TaskEmployeeProgresses
                    .Where(p => p.EmployeeId == employeeId && p.OrgId == orgId)
                    .Include(p => p.Task)
                        .ThenInclude(t => t!.Post)
                    .Include(p => p.Task)
                        .ThenInclude(t => t!.ReviewerEmployee)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(p => p.Status == status);

                if (priority.HasValue)
                    query = query.Where(p => p.Task != null && p.Task.Priority == priority.Value);

                return await query
                    .OrderBy(p => p.Status == TaskStatuses.Completed ? 1 : 0)   // completed last
                    .ThenBy(p => p.Task!.DueDate)                     // nearest due first
                    .ToListAsync();
            }

            // ════════════════════════════════════════════════════
            // REVIEWING TASKS (reviewer view)
            // ════════════════════════════════════════════════════

            public async Task<List<TaskEmployeeProgress>> GetReviewingTasks(int reviewerEmployeeId, int orgId)
            {
                return await _context.TaskEmployeeProgresses
                    .Where(p => p.OrgId == orgId
                            && p.Task.ReviewerEmployeeId == reviewerEmployeeId
                            && p.Status != TaskStatuses.Completed
                            && !p.Task.IsDeleted)
                    .Include(p => p.Task)
                        .ThenInclude(t => t!.Post)
                    .Include(p => p.Task)
                        .ThenInclude(t => t!.AssignedByEmployee)
                    .Include(p => p.Employee)
                    .Include(p => p.ReviewedByEmployee)
                    .AsNoTracking()
                    .OrderByDescending(p => p.UpdatedAt)
                    .ToListAsync();
            }

        public async Task<int?> GetDepartmentHeadEmployeeId(int taskId, int orgId)
        {
            // Get departmentId from first employee in the task's progress rows
            var firstEmpId = await _context.TaskEmployeeProgresses
                .AsNoTracking()
                .Where(p => p.TaskId == taskId && p.OrgId == orgId)
                .Select(p => (int?)p.EmployeeId)
                .FirstOrDefaultAsync();

            if (firstEmpId == null) return null;

            var deptId = await _context.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == firstEmpId && e.OrganizationId == orgId)
                .Select(e => (int?)e.DepartmentId)
                .FirstOrDefaultAsync();

            if (deptId == null) return null;

            return await _context.Departments
        .AsNoTracking()
        .Where(d => d.DepartmentId == deptId && d.OrganizationId == orgId && d.IsActive)
        .Select(d => d.HeadOfDepartmentId)
        .FirstOrDefaultAsync();
}


public async Task UpdateReviewerEmployee(int taskId, int orgId, int reviewerEmployeeId)
{
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == taskId && t.OrgId == orgId && !t.IsDeleted);

    if (task == null) return;

    task.ReviewerEmployeeId = reviewerEmployeeId;
    task.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
}
    }
}
