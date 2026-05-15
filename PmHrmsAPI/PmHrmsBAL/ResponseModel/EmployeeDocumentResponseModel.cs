namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class EmployeeDocumentResponseModel
    {
        public int DocumentId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public int DocumentMasterId { get; set; } 
        public string? DocumentDisplayName { get; set; }

        public string? DocumentType { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime? UploadDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public string? VerificationStatus { get; set; }
        public string? HrRemarks { get; set; }

        public int? VerifiedById { get; set; }
        public string? VerifiedByName { get; set; } 
        public DateTime? VerifiedDate { get; set; }
    }
}
