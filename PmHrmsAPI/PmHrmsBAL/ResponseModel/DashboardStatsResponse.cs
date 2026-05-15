namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class DashboardStatsResponse
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalDesignations { get; set; }
        
        public dynamic RecentDepartments { get; set; } = new List<dynamic>(); 
    }
}