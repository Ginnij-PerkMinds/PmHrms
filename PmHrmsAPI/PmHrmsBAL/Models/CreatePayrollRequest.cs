using System;

namespace PmHrmsAPI.PmHrmsBAL.Models
{
    public class CreatePayrollRequest
    {
        public int OrgId { get; set; }
        public byte? PayrollMonth { get; set; }
        public short? PayrollYear { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? RunType { get; set; }
        public DateOnly? ScheduledRunDate { get; set; }

        public string? TriggeredBy { get; set; }
    }
}
