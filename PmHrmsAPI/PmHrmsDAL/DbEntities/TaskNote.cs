namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class TaskNote
    {
        public int Id { get; set; }
        public int OrgId { get; set; }

        public int TaskId { get; set; }
        public string Content { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }

        // Optional @mention — "Gaurav, please handle this additionally"
        public int? MentionedEmployeeId { get; set; }

        public DateTime CreatedAt { get; set; }

        // ── Navigation ──────────────────────────────────────
        public TaskEntity Task { get; set; } = null!;
        public Employee? CreatedByEmployee { get; set; }
        public Employee? MentionedEmployee { get; set; }
    }
}