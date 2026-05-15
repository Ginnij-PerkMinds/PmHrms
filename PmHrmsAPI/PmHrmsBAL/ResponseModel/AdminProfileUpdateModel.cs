namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class AdminProfileUpdateModel
    {
        public int EmployeeId { get; set; }
        public int OrganizationId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? OfficialEmail { get; set; }
        public string? Password { get; set; }
        public int? PolicyId { get; set; }
         public int? AssignedOfficeId { get; set; }
    }

}
