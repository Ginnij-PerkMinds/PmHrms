namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class ClockInRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
    }
}
