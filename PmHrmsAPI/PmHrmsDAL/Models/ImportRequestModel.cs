using Microsoft.AspNetCore.Routing.Constraints;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class ImportRequestModel
    {
        public string EntityType { get; set; } = string.Empty;
        public Dictionary<string, string> Mapping { get; set; } = new();
        public List<Dictionary<string, string?>> Rows { get; set; } = new();
        public String FileName { get; set; }
    }
}
