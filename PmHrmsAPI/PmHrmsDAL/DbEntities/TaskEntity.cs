namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public int? PostId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public byte Priority { get; set; }
        public int AssignedByUserId { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        
        public string ReviewerType { get; set; } = "Self";
        public int? ReviewerEmployeeId { get; set; }   
        public string? ReviewRemarks { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

       
        public Post? Post { get; set; }
        public Employee? AssignedByEmployee { get; set; }
        public Employee? ReviewerEmployee { get; set; }   // FK → ReviewerEmployeeId

        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskEmployeeProgress> Progress { get; set; } = new List<TaskEmployeeProgress>();
        public ICollection<TaskFollowUp> FollowUps { get; set; } = new List<TaskFollowUp>();
        public ICollection<TaskNote> Notes { get; set; } = new List<TaskNote>();
    }
}