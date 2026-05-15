using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class OfficeLocationDAL
    {
        private readonly PmHrmsContext _context;
        private readonly ILogger<OfficeLocationDAL> _logger;

        public OfficeLocationDAL(
            PmHrmsContext context,
            ILogger<OfficeLocationDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OfficeLocation?> GetDefaultLocationAsync(int orgId)
        {
            return await _context.OfficeLocations
                .Where(x => x.OrganizationId == orgId && x.IsDefault)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetDefaultLocationAsync(int locationId, int orgId)
        {
            var locations = await _context.OfficeLocations
                .Where(x => x.OrganizationId == orgId)
                .ToListAsync();

            foreach (var loc in locations)
            {
                loc.IsDefault = loc.LocationId == locationId;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<OfficeLocation>> GetByOrgIdAsync(int orgId)
        {
            try
            {
                _logger.LogInformation("Fetching office locations for OrgId: {OrgId}", orgId);

                var data = await _context.OfficeLocations
                    .Where(x => x.OrganizationId == orgId)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Found {Count} office locations for OrgId: {OrgId}", data.Count, orgId);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching office locations for OrgId: {OrgId}", orgId);
                throw;
            }
        }

        public async Task<OfficeLocation> AddAsync(OfficeLocation location)
        {
            try
            {
                _logger.LogInformation("Adding office location for OrgId: {OrgId}", location.OrganizationId);

                await _context.OfficeLocations.AddAsync(location);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Office location added successfully. LocationId: {LocationId}", location.LocationId);

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding office location");
                throw;
            }
        }

        public async Task<OfficeLocation?> UpdateAsync(int id, OfficeLocation model)
        {
            var existing = await _context.OfficeLocations.FindAsync(id);

            if (existing == null)
                return null;

            existing.LocationName = model.LocationName;
            existing.Latitude = model.Latitude;
            existing.Longitude = model.Longitude;
            existing.GeoRadiusMeters = model.GeoRadiusMeters;
            existing.AllowedIpAddress = model.AllowedIpAddress;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting office location Id: {Id}", id);

                var entity = await _context.OfficeLocations.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Office location not found. Id: {Id}", id);
                    return false;
                }

                _context.OfficeLocations.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Office location deleted successfully. Id: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting office location Id: {Id}", id);
                throw;
            }
        }
    }
}