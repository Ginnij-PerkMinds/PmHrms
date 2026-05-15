namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class AutoMappingRequest
    {
        public List<string> ExcelColumns { get; set; }
        public List<SystemField> SystemFields { get; set; }
        
        public List<Dictionary<string, object>> SampleData { get; set; }
    }
}