namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class TaskNoteModel
    {
        public string Content { get; set; } = string.Empty;
        public int? MentionedEmployeeId { get; set; }
    }
}