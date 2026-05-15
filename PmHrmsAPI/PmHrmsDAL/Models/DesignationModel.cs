using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class DesignationModel
    {
        [Required(ErrorMessage = "Designation name is required")]
        [MinLength(2, ErrorMessage = "Minimum 2 characters required")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters allowed")]
        [RegularExpression(@"^[a-zA-Z &\-]{2,50}$",
            ErrorMessage = "Only letters, spaces, & and - are allowed")]
        public string DesignationName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Hierarchy level must be between 1 and 10")]
        public int? HierarchyLevel { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }
    }
}
