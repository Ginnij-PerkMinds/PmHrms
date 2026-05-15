using System;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class PayrollRunResponse
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public byte PayrollMonth { get; set; }
        public short PayrollYear { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public PayrollRunType RunType { get; set; } 
        public bool IsLocked { get; set; }
        public DateTime? ActualRunStart { get; set; }
        public DateTime? ActualRunEnd { get; set; }
        public int? TotalEmployees { get; set; }
        public decimal? TotalNetPayable { get; set; }
        public string? TriggeredBy { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
