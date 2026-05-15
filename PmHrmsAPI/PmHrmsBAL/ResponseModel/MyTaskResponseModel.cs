namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class MyTaskResponseModel
    {
        // Progress row identity
        public int ProgressId { get; set; }

        // Task details
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public string? PostTitle { get; set; }

        // My personal status on this task
        public string Status { get; set; } = "Pending";
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Remarks { get; set; }

        // Reviewer info (so employee knows who will review)
        public string ReviewerType { get; set; } = "Self";
        public string? ReviewerName { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ReviewingTaskResponseModel
    {
        public int ProgressId { get; set; }
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public string? PostTitle { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        public string OverallTaskStatus { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string AssignedByName { get; set; } = string.Empty;

        // Progress summary for reviewer
        public int TotalAssigned { get; set; }
        public int TotalCompleted { get; set; }
        public int ProgressPercent { get; set; }

        public string? EmployeeRemarks { get; set; }
        public DateTime StatusUpdatedAt { get; set; }

        // Reviewer's own remarks/decision fields on this progress row
        public string? ReviewRemarks { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
