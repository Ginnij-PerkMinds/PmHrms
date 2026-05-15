using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IOfficeLocationBAL
    {
        Task<List<OfficeLocation>> GetLocations(int orgId);
        Task<OfficeLocation> AddLocation(int orgId, OfficeLocation model);
        Task<bool> DeleteLocation(int id);
        Task<OfficeLocation?> UpdateLocation(int id, OfficeLocation model);

        Task<bool> SetDefaultLocation(int locationId, int orgId);

    }
}
