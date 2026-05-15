namespace PmHrmsAPI.PmHrmsDAL.Models
{
    

    public class ReuploadDocumentRequest
    {
        public int EmployeeId { get; set; }
        public int DocumentMasterId { get; set; }
        public string DocumentType { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public IFormFile File { get; set; }
    }


}
