namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class PostModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PostType { get; set; } = "General";   // Announcement | Notice | PolicyUpdate | General
        public bool IsPublished { get; set; } = false;
        public DateTime? VisibleFrom { get; set; }
        public DateTime? VisibleUntil { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
        public List<PostTargetModel> Targets { get; set; } = new();
    }

    public class PostTargetModel
    {
        public string TargetType { get; set; } = string.Empty;  // Employee | Department | Designation | All
        public int? TargetId { get; set; }                       // null when TargetType = All
    }
}