namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    // ── Post ─────────────────────────────────────────────────────────
    public class PostResponseModel
    {
        public int PostId { get; set; }
        public int OrgId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PostType { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PostTargetResponseModel> Targets { get; set; } = new();
        public int LinkedTaskCount { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? VisibleFrom { get; set; }
        public DateTime? VisibleUntil { get; set; }
    }

    public class PostTargetResponseModel
    {
        public int PostTargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int? TargetId { get; set; }
        public string? TargetName { get; set; }     // e.g. dept name or "All Employees"
    }

    // ── Task ─────────────────────────────────────────────────────────
    public class TaskResponseModel
    {
        public int TaskId { get; set; }
        public int OrgId { get; set; }
        public int? PostId { get; set; }
        public string? PostTitle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedByName { get; set; } = string.Empty;

        public string ReviewerType { get; set; } = "Self";
        public int? ReviewerEmployeeId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewRemarks { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalAssigned { get; set; }

        public int TotalCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public List<TaskAssignmentResponseModel> Assignments { get; set; } = new();
    }

    public class TaskAssignmentResponseModel
    {
        public int AssignmentId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int? TargetId { get; set; }
        public string? TargetName { get; set; }
    }

    // ── Progress ─────────────────────────────────────────────────────
    public class TaskProgressResponseModel
    {
        public int ProgressId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? Remarks { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public int? SourceId { get; set; }
        public string ReviewerType { get; set; } = "Self";
        public int? ReviewerEmployeeId { get; set; }
        public string? ReviewerName { get; set; }
    }

    // ── Follow-up ─────────────────────────────────────────────────────
    public class TaskFollowUpResponseModel
    {
        public int FollowUpId { get; set; }
        public int TaskId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReceipts { get; set; }
        public int ReadCount { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
