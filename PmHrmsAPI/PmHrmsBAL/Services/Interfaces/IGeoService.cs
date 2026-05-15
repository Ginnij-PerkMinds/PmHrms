namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IGeoService
    {
        double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2);
    }
}
