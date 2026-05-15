namespace PmHrmsAPI.PmHrmsDAL.Models
{
   
   
    public class DocumentVerificationModel
    {
        public int DocumentId { get; set; }
        public string VerificationStatus { get; set; } = "Verified"; // Verified / Rejected
       // public int VerifiedById { get; set; } // HR ki ID
        public string? HrRemarks { get; set; }
    }
}