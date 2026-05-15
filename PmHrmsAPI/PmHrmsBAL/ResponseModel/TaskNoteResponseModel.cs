namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class TaskNoteResponseModel
    {
        public int NoteId { get; set; }
        public int TaskId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
        public int? MentionedEmployeeId { get; set; }
        public string? MentionedEmployeeName { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public string? ReviewerName { get; set; }
        public int ProgressPercent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}