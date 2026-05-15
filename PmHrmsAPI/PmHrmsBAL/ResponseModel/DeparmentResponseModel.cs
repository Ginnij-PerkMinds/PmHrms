namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class DepartmentResponseModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? HeadOfDepartmentId { get; set; }

        public int OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        public int EmployeeCount { get; set; }
        public List<DesignationResponseModel> Designations { get; set; } = new();
    }
}