namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class TaskEmployeeProgress
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
 
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
 
        public string Status { get; set; } = "Pending";          
        public DateTime? CompletedAt { get; set; }
        public string? Remarks { get; set; }
        public string? ReviewRemarks { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByEmployeeId { get; set; }
 
        public string SourceType { get; set; } = string.Empty;   
        public int? SourceId { get; set; }
 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
 
       
        public TaskEntity Task { get; set; } = null!;
 
       
        public Employee Employee { get; set; } = null!;
        public Employee? ReviewedByEmployee { get; set; }
    }
}
