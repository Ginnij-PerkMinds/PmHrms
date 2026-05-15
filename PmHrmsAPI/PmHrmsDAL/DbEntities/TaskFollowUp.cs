namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class TaskFollowUp
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
 
        public int TaskId { get; set; }
 
        public string Message { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
 
        public string TargetType { get; set; } = "Pending";      // All | Pending | Specific
 
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledAt { get; set; }
 
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
 
        public DateTime CreatedAt { get; set; }
 
        public TaskEntity Task { get; set; } = null!;
 
        
        public Employee? CreatedByEmployee { get; set; }
 
        
        public ICollection<TaskFollowUpReceipt> Receipts { get; set; } = new List<TaskFollowUpReceipt>();
    }
}