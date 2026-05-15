namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class TaskModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; } = 2;          // 1=Low 2=Medium 3=High 4=Critical
        public DateTime DueDate { get; set; }
        public int? PostId { get; set; }                // optional link to a Post
       public string ReviewerType { get; set; } = "Self";
        public int? ReviewerEmployeeId { get; set; }

        public List<TaskAssignmentModel> Assignments { get; set; } = new();
    }

    public class TaskAssignmentModel
    {
        public string TargetType { get; set; } = string.Empty;  // Employee | Department | Designation | All
        public int? TargetId { get; set; }
    }

    public class UpdateTaskStatusModel
    {
        public string Status { get; set; } = string.Empty;      // Pending | InProgress | ReviewRequested
        public string? Remarks { get; set; }
    }

    public class UpdateTaskReviewModel
    {
        public string Status { get; set; } = string.Empty;      // UnderReview | Completed
        public string? Remarks { get; set; }
    }

    public class TaskFollowUpModel
    {
        public string Message { get; set; } = string.Empty;
        public string TargetType { get; set; } = "Pending";     // All | Pending | Specific
        public bool IsScheduled { get; set; } = false;
        public DateTime? ScheduledAt { get; set; }
    }
}
