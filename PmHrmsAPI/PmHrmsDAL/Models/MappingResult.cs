namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class MappingResult
    {
        public string Column { get; set; }
        public double Confidence { get; set; } // 0 to 1
        public string MatchSource { get; set; } // "Header" or "Content"
    }
}
