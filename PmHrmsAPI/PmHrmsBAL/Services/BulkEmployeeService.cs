using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public class BulkEmployeeService : IBulkEmployeeService
    {
        private readonly PmHrmsContext _db;
        private readonly IEmployeeOnboardingService _onboarding;
        private readonly ILogger<BulkEmployeeService> _logger;

       public BulkEmployeeService(
            PmHrmsContext db,
            IEmployeeOnboardingService onboarding,
            ILogger<BulkEmployeeService> logger)
        {
            _db         = db;
            _onboarding = onboarding;
            _logger     = logger;
        }


        public async Task BulkInsertAsync(
              int orgId,
              List<EmployeeModel> employees,
              int systemRoleId,
              int orgRoleId,
              string? sharedPassword = null) 
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
               
                var empEntities = employees.Select(e => new Employee
                {
                    OrganizationId = orgId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    OfficialEmail = e.OfficialEmail,
                    EmployeeCode = e.EmployeeCode,
                    PhoneNumber = e.PhoneNumber,
                    DateOfJoining = e.DateOfJoining,
                    EmploymentStatus = e.EmploymentStatus,
                    WorkMode = e.WorkMode,
                    DepartmentId = e.DepartmentId,
                    DesignationId = e.DesignationId,
                    AssignedOfficeId = e.AssignedOfficeId == 0 ? null : e.AssignedOfficeId,
                    PolicyId         = e.PolicyId == 0 ? null : e.PolicyId,
                    IsActive         = true
                }).ToList();                      

                _db.Employees.AddRange(empEntities);
                await _db.SaveChangesAsync();

                 _logger.LogInformation("[BulkInsert] {Count} employees created | OrgId {OrgId}",empEntities.Count, orgId);
 
                     await _onboarding.ApplyDefaultsAsync(empEntities, orgId);
                         await _db.SaveChangesAsync(); 
 
                _logger.LogInformation("[BulkInsert] Defaults applied for {Count} employees",
                    empEntities.Count);

                // 2️⃣ Create AppUsers with shared password
                var passwordToUse = sharedPassword ?? Guid.NewGuid().ToString("N")[..10];

                var users = empEntities.Select(emp => new AppUser
                {
                    EmployeeId = emp.EmployeeId,
                    SystemRoleId = systemRoleId,
                    OrgRoleId = orgRoleId,
                    RoleId = 2,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordToUse),
                    IsFirstLogin = true
                }).ToList();

                _db.AppUsers.AddRange(users);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
             _logger.LogInformation("[BulkInsert] COMPLETE | Employees: {Count} | OrgId: {OrgId}",
                    empEntities.Count, orgId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "[BulkInsert] FAILED | OrgId: {OrgId} | Error: {Error}",
                    orgId, ex.Message);
                throw;
            }
        }
    }
}
 