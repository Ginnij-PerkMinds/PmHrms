namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{                                    
     public class PostTarget
    {
        public int Id { get; set; }      
        public int OrgId { get; set; }
 
        public int PostId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int? TargetId { get; set; }
 
       
        public Post Post { get; set; } = null!;
    }
}
 