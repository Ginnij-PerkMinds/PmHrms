using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class RemoteLocationDAL
    {

        private readonly PmHrmsContext _context;
        private readonly ILogger<OfficeLocationDAL> _logger;

        public RemoteLocationDAL(PmHrmsContext context,
            ILogger<OfficeLocationDAL> logger)
                {
                    _context = context;
                    _logger = logger;
                }


        public async Task<EmployeeRemoteLocation?> GetActiveRemoteAsync(int employeeId)
        {
            return await _context.EmployeeRemoteLocations
                .FirstOrDefaultAsync(r =>
                    r.EmployeeId == employeeId &&
                    r.IsActive == true);
        }

        public async Task AddRemoteAsync(EmployeeRemoteLocation remote)
        {
            await _context.EmployeeRemoteLocations.AddAsync(remote);
        }

        internal async Task AddRemoteLocationAsync(int employeeId, double lat, double lng)
        {
            throw new NotImplementedException();
        }
    }
}
