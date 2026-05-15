namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class EmployeeDocumentSummaryResponseModel
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public int RequiredCount { get; set; }
        public int UploadedCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public bool HasUploadedDocuments => UploadedCount > 0;
    }
}
