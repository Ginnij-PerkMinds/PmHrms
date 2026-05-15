using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class LocationRequest
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        public string? WorkType { get; set; }
    }
}
