namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
     public class TaskAssignment
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
 
        public int TaskId { get; set; }
 
        public string TargetType { get; set; } = string.Empty;   
        public int? TargetId { get; set; }
 
       
        public TaskEntity Task { get; set; } = null!;
    }
}