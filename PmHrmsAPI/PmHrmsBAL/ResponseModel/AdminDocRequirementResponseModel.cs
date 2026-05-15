namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AdminDocRequirementResponseModel
    {
        public int DocumentMasterId { get; set; }
        public string DisplayName { get; set; } = null!;
        public bool IsSelected { get; set; }
        public bool IsMandatory { get; set; }
    }
}