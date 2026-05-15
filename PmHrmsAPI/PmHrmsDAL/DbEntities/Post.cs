namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
   public class Post
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
 
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PostType { get; set; } = "General";
 
        public int CreatedByUserId { get; set; }
 
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
 
        
        public ICollection<PostTarget> Targets { get; set; } = new List<PostTarget>();
 
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
 
        public Employee? CreatedByEmployee { get; set; }

        public string? ImagePath { get; set; }
        public DateTime? VisibleFrom { get; set; }
        public DateTime? VisibleUntil { get; set; }
    }
}
 