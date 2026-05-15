namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class TaskFollowUpReceipt
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
 
        public int FollowUpId { get; set; }
        public int EmployeeId { get; set; }
 
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
 
        // ── Navigation ──────────────────────────────────────
        public TaskFollowUp FollowUp { get; set; } = null!;
    }
}
 