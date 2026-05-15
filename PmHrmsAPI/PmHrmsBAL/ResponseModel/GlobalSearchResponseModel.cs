namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class GlobalSearchResponseModel
    {
        public List<GlobalSearchEmployeeResponseModel> Employees { get; set; } = new();
        public List<GlobalSearchDepartmentResponseModel> Departments { get; set; } = new();
        public List<GlobalSearchDesignationResponseModel> Designations { get; set; } = new();
        public List<GlobalSearchDocumentResponseModel> Documents { get; set; } = new();
        public int Total { get; set; }
    }

    public class GlobalSearchEmployeeResponseModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string OfficialEmail { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class GlobalSearchDepartmentResponseModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class GlobalSearchDesignationResponseModel
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? HierarchyLevel { get; set; }
    }

    public class GlobalSearchDocumentResponseModel
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string DocumentTypeName { get; set; }
        public int RequiredCount { get; set; }
        public int UploadedCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
