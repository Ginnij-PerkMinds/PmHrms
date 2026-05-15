using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    
    public class SalaryStructureDAL
    {
        private readonly PmHrmsContext _context;
        private readonly ILogger<SalaryStructureDAL> _logger;

        public SalaryStructureDAL(PmHrmsContext context, ILogger<SalaryStructureDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        public async Task<(List<SalaryStructure>, int)> GetAll(
            int page, int size, string? search, int orgId)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var query = _context.SalaryStructures
                .Include(x => x.Components)
                    .ThenInclude(c => c.SalaryComponentMaster) // Load master for display
                .Where(x => x.OrganizationId == orgId && x.IsActive);

            // Optional search filter on structure name
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.StructureName.Contains(search));

            int totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (data, totalCount);
        }

       
        public async Task<SalaryStructure?> GetById(int id)
        {
            return await _context.SalaryStructures
                .Include(x => x.Components)
                  .ThenInclude(c => c.SalaryComponentMaster)  
                .FirstOrDefaultAsync(x => x.SalaryStructureId == id);
        }

        
        public async Task<SalaryStructure> Add(SalaryStructure entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.SalaryStructures.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        
        public async Task<SalaryStructure?> Update(SalaryStructure entity, List<SalaryComponent> components)
{
    var existing = await _context.SalaryStructures
        .Include(x => x.Components)
        .FirstOrDefaultAsync(x => x.SalaryStructureId == entity.SalaryStructureId);

    if (existing == null)
        return null;

    _context.Entry(existing).CurrentValues.SetValues(entity);

    var oldComponents = existing.Components.ToList();
    _context.SalaryComponents.RemoveRange(oldComponents);

    existing.Components = new List<SalaryComponent>();

     foreach (var comp in components)
    {
        existing.Components.Add(new SalaryComponent
        {
            ComponentMasterId = comp.ComponentMasterId,
            ComponentName = comp.ComponentName,
            Amount = comp.Amount,
            IsEarning = comp.IsEarning,
            OrganizationId = existing.OrganizationId
        });
    }

    await _context.SaveChangesAsync();
    return existing;
}

        
        public async Task<bool> Delete(int id)
        {
            var entity = await _context.SalaryStructures.FindAsync(id);
            if (entity == null)
                return false;

            entity.IsActive = false;
            

            await _context.SaveChangesAsync();
            return true;
        }

       
        public async Task RemoveExistingDefault(int orgId)
        {
            var defaults = await _context.SalaryStructures
                .Where(x => x.OrganizationId == orgId && x.IsDefault && x.IsActive)
                .ToListAsync();

            if (!defaults.Any())
                return;

            foreach (var d in defaults)
                d.IsDefault = false;

            await _context.SaveChangesAsync();
        }

                public async Task AssignToDesignation(int designationId, int salaryStructureId, int orgId)
        {
            var existing = await _context.DesignationSalaryMappings
                .FirstOrDefaultAsync(x => x.DesignationId == designationId && x.OrganizationId == orgId);

            if (existing == null)
            {
                await _context.DesignationSalaryMappings.AddAsync(new DesignationSalaryMapping
                {
                    DesignationId = designationId,
                    SalaryStructureId = salaryStructureId,
                    OrganizationId = orgId
                });
            }
            else
            {
                existing.SalaryStructureId = salaryStructureId;
            }

            await _context.SaveChangesAsync();
        }

        
        public async Task<List<DesignationSalaryMapping>> GetDesignationMappings()
        {
            return await _context.DesignationSalaryMappings
                .Include(x => x.Designation)
                .Include(x => x.SalaryStructure)
                .Where(x => x.Designation.IsActive && x.SalaryStructure.IsActive)
                .ToListAsync();
        }


            public async Task<List<SalaryComponentMaster>> GetMastersByIds(List<int> ids)
            {
                return await _context.SalaryComponentMasters
                    .AsNoTracking()
                    .Where(m => ids.Contains(m.Id))
                    .ToListAsync();
            }
       
        public async Task<List<SalaryComponentMaster>> LoadMaster()
        {
            try
            {
                _logger.LogInformation($"[SalaryStructure LoadMaster DAL] Executing query to load salary component masters");
                
                var result = await _context.SalaryComponentMasters
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.ComponentName)
                    .ToListAsync();
                
                _logger.LogInformation($"[SalaryStructure LoadMaster DAL] Query completed - Found {result.Count} active salary component masters");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SalaryStructure LoadMaster DAL] Database error - {ex.Message}\\nStack Trace: {ex?.StackTrace}");
                throw;
            }
        }

        
        public async Task<SalaryStructure?> GetEmployeeSalary(int employeeId, int orgId)
        {
            // Fetch the employee
            var employee = await _context.Employees
                .IgnoreQueryFilters() 
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.OrganizationId == orgId);
            if (employee == null)
                throw new Exception("Employee not found");

            // Level 1: Employee-specific salary structure
            if (employee.SalaryStructureId.HasValue)
            {
                var empSalary = await _context.SalaryStructures
                    .IgnoreQueryFilters()
                    .Include(x => x.Components)
                        .ThenInclude(c => c.SalaryComponentMaster)
                    .FirstOrDefaultAsync(s =>
                        s.SalaryStructureId == employee.SalaryStructureId.Value &&
                        s.OrganizationId == orgId &&
                        s.IsActive);

                if (empSalary != null)
                    return empSalary;
            }


            // Level 2: Designation-level salary structure
            if (employee.DesignationId.HasValue)
            {
                var mapping = await _context.DesignationSalaryMappings
                    .FirstOrDefaultAsync(m => m.DesignationId == employee.DesignationId && m.OrganizationId == orgId);

                if (mapping != null)
                {
                    var desigSalary = await _context.SalaryStructures
                        .IgnoreQueryFilters()
                        .Include(x => x.Components)
                            .ThenInclude(c => c.SalaryComponentMaster)
                        .FirstOrDefaultAsync(s =>
                            s.SalaryStructureId == mapping.SalaryStructureId &&
                            s.OrganizationId == orgId &&
                            s.IsActive);

                    if (desigSalary != null)
                        return desigSalary;
                }
            }

            // Level 3: Organization default
            var defaultSalary = await _context.SalaryStructures
                .IgnoreQueryFilters()
                .Include(x => x.Components)
                    .ThenInclude(c => c.SalaryComponentMaster)
                .Where(s =>
                    s.OrganizationId == orgId &&
                    s.IsDefault &&
                    s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            return defaultSalary;
        }
    }
}