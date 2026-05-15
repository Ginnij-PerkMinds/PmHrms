using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class EmployeeDocumentModel
    {
        [Required]
        public int EmployeeId { get; set; }

        public int DocumentMasterId { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public string DocumentPath { get; set; } = string.Empty; 

        public DateOnly? ExpiryDate { get; set; }
    }
}