namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class DesignationResponseModel
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int? HierarchyLevel { get; set; }

        public string? WorkPolicyName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? OrganizationName { get; set; }
        public bool IsSystemDefault { get; set; }
    }
}
