namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class OrganizationDocumentRequirementResponseModel
    {
        public int RequirementId { get; set; }
        public int OrganizationId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int DocumentMasterId { get; set; }  

        public bool IsMandatory { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}