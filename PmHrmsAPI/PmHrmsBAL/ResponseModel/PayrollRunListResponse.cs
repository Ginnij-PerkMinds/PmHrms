using System;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class PayrollRunListResponse
    {
        public int Id { get; set; }
        public string Period { get; set; } = string.Empty; // e.g. "March 2026"
         public DateOnly StartDate { get; set; }      
         public DateOnly EndDate { get; set; }   
        public string Status { get; set; } = string.Empty;
        public string? TriggeredBy { get; set; }     
        public int TotalEmployees { get; set; }
        public decimal TotalNetPayable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
