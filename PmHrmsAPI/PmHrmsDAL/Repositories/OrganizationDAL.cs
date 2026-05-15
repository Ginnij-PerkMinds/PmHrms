using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class OrganizationDAL
    {
        private const string CustomHolidayCountryCode = "CSTM";

        private readonly PmHrmsContext _context;

        public OrganizationDAL(PmHrmsContext context)
        {
            _context = context;
        }


        public async Task<(List<Organization>, int totalCount)> GetAllOrganization(int pageNumber, int pageSize, string? searchTerm)
        {
            var query =  _context.Organizations
                       .Where(o => o.IsActive)         
                        .Include(o => o.State)
                         .Include(o => o.Country)
                         .AsNoTracking()
                          .AsQueryable();


            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => o.OrganizationName.Contains(searchTerm) ||
                                         o.OfficialEmail.Contains(searchTerm));
                
            }

            int totalCount = await query.CountAsync();

            var data = await query
                            .OrderByDescending(o => o.OrgId) 
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();


            return (data, totalCount);


        }


        public async Task<Organization?> GetOrganization(int id)
        {
            return await _context.Organizations
                        .Include(o => o.State)
                         .Include(o => o.Country)
                         .AsNoTracking()
                          .FirstOrDefaultAsync(o => o.OrgId == id);
        }




        public async Task<Organization?> AddOrganization(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
            return organization;

        }


        public async Task<Organization?> UpdateOrganization(Organization organization)
        {
            var existingOrg = await _context.Organizations.FindAsync(organization.OrgId);

            if (existingOrg == null)
            {
                return null;
            }

         

         
            existingOrg.OrganizationName = organization.OrganizationName;
            existingOrg.OfficialEmail = organization.OfficialEmail;
            existingOrg.ContactPhoneNo = organization.ContactPhoneNo;
            existingOrg.WebsiteUrl = organization.WebsiteUrl;
            existingOrg.AddressLine1 = organization.AddressLine1; 
            existingOrg.AddressLine2 = organization.AddressLine2;
            existingOrg.City = organization.City;
            existingOrg.ZipCode = organization.ZipCode;
            existingOrg.LogoUrl = organization.LogoUrl;

            await _context.SaveChangesAsync();
            return existingOrg;
        }

        public async Task<bool> UpdateOrganizationLogo(int orgId, string? logoUrl)
        {
            var existingOrg = await _context.Organizations.FindAsync(orgId);
            if (existingOrg == null)
            {
                return false;
            }

            existingOrg.LogoUrl = logoUrl;
            await _context.SaveChangesAsync();
            return true;
        }
        




        public async Task<bool> IsOrgSetupCompleted(int orgId)
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(o => o.OrgId == orgId)
                .Select(o => o.IsSetupCompleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasDepartments(int orgId)
        {
            return await _context.Departments
                .AsNoTracking()
               .AnyAsync(d =>
                  d.OrganizationId == orgId &&
                 !d.IsSystemDefault
        );
        }

        public async Task<bool> HasDesignations(int orgId)
        {
            return await _context.Designations
                .AsNoTracking()
                 .AnyAsync(d =>
                  d.Department.OrganizationId == orgId &&
                  !d.IsSystemDefault
        );
        }



        public async Task<Employee?> GetAdminEmployee(int orgId)
        {
            return await _context.Employees
                .AsNoTracking() 
                .Include(e => e.Organization) 
                .Where(e => e.OrganizationId == orgId)
                .OrderBy(e => e.DateOfJoining) 
                .FirstOrDefaultAsync();
        }


        public async Task<AppUser?> GetUserByEmployeeId(int empId)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(x => x.EmployeeId == empId);
        }

        public async Task UpdateUser(AppUser user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }




        public async Task MarkOrgSetupCompleted(int orgId)
        {
            var org = await _context.Organizations.FindAsync(orgId);
            if (org == null) return;

            org.IsSetupCompleted = true;
             org.IdGhost = false;
             org.CreatedByIp = null;
             
            await _context.SaveChangesAsync();
        }







        public async Task<bool> DeleteOrganization(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return false;
            }
            organization.IsActive = false;   //  Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetAvailableHolidayYears()
            {
                var dbYears = await _context.SystemHolidays
                    .AsNoTracking()
                    .Select(h => h.Year)
                    .Distinct()
                    .ToListAsync();
            
                var currentYear = DateTime.Now.Year;
            
                
                var allYears = new HashSet<int>(dbYears)
                {
                    currentYear,
                    currentYear + 1
                };
            
                return allYears
                    .OrderByDescending(y => y)
                    .ToList();
            }

        public async Task<List<SystemHoliday>> GetOfficialSystemHolidaysByYear(int year)
        {
            return await _context.SystemHolidays
                .AsNoTracking()
                .Where(h => h.Year == year && h.CountryCode != CustomHolidayCountryCode)
                .OrderBy(h => h.HolidayDate)
                .ThenBy(h => h.HolidayName)
                .ToListAsync();
        }

        

        public async Task<List<OfficeLocation>> GetOfficeLocationsByOrg(int orgId)
        {
            return await _context.OfficeLocations
                .AsNoTracking()
                .Where(x => x.OrganizationId == orgId)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.LocationName)
                .ToListAsync();
        }

        

       

        

     
public async Task<(int empCount, int deptCount, int desigCount, dynamic recentDepts)> GetDashboardStatsAsync(int orgId)
{
    
    var empCount = await _context.Employees
        .Where(e => e.OrganizationId == orgId)
        .CountAsync();

    
    var deptQuery = _context.Departments
        .Where(d => d.OrganizationId == orgId && !d.IsSystemDefault);

    var deptCount = await deptQuery.CountAsync();

    
    var desigCount = await _context.Designations
        .Where(d => d.Department.OrganizationId == orgId && !d.IsSystemDefault)
        .CountAsync();

    
    var recentDepts = await deptQuery
        .OrderByDescending(d => d.DepartmentId)
        .Take(3)
        
        .Select(d => new {
            d.DepartmentId,
            d.DepartmentName,
            Designations = d.Designations
                .Where(des => des.IsActive ) 
                .Select(des => des.DesignationName)
                .Take(3)
                .ToList()
            
        })
        .ToListAsync();

    return (empCount, deptCount, desigCount, recentDepts);
}

        

        

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
