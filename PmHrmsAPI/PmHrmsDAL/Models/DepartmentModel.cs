using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class DepartmentModel
    {
        [Required(ErrorMessage = "Department Name is required")]
        [MinLength(2, ErrorMessage = "Minimum 2 characters required")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters allowed")]
        [RegularExpression(@"^[a-zA-Z &-]{2,50}$",
            ErrorMessage = "Only letters, spaces, & and - are allowed")]
        public string DepartmentName { get; set; } = string.Empty;

        public int? HeadOfDepartmentId { get; set; }

        [Required(ErrorMessage = "Organization ID is required")]
        public int OrganizationId { get; set; }
    }
}
