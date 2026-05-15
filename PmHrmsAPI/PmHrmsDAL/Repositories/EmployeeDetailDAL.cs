using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class EmployeeDetailDAL
    {
        private readonly PmHrmsContext _context;

        public EmployeeDetailDAL(PmHrmsContext context)
        {
            _context = context;
        }

       
        public async Task<EmployeeDetail?> GetDetailByEmployeeId(int employeeId)
        {
            if (employeeId <= 0)
                return null;

            return await _context.EmployeeDetails
                .Include(d => d.CurrentState)
                .Include(d => d.CurrentCountry)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.EmployeeId == employeeId);
        }

 
        public async Task<EmployeeDetail> AddDetail(EmployeeDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            await _context.EmployeeDetails.AddAsync(detail);
            await _context.SaveChangesAsync();

            return detail;
        }

   
        public async Task<EmployeeDetail?> UpdateDetail(EmployeeDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            var existing = await _context.EmployeeDetails
                .FirstOrDefaultAsync(d => d.DetailId == detail.DetailId);

            if (existing == null)
                return null;

           
            existing.DateOfBirth = detail.DateOfBirth;
            existing.BloodGroup = detail.BloodGroup;
            existing.MaritalStatus = detail.MaritalStatus;
            existing.FatherName = detail.FatherName;
            existing.PanNumber = detail.PanNumber;
            existing.AadharNumber = detail.AadharNumber;
            existing.PassportNumber = detail.PassportNumber;
            existing.CurrentAddressLine = detail.CurrentAddressLine;
            existing.CurrentCity = detail.CurrentCity;
            existing.CurrentStateId = detail.CurrentStateId;
            existing.CurrentCountryId = detail.CurrentCountryId;
            existing.CurrentZipCode = detail.CurrentZipCode;
            existing.LinkedinUrl = detail.LinkedinUrl;
            existing.GithubUrl = detail.GithubUrl;

            await _context.SaveChangesAsync();

            return existing;
        }

      
        public async Task<bool> DetailExists(int employeeId)
        {
            return await _context.EmployeeDetails
                .AnyAsync(d => d.EmployeeId == employeeId);
        }
    }
}
