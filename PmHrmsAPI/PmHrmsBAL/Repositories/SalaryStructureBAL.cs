using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

public class SalaryStructureBAL : ISalaryStructureBAL
{
    private readonly SalaryStructureDAL _dal;
    private readonly ITenantService _tenantService;
    private readonly ILogger<SalaryStructureBAL> _logger;

    public SalaryStructureBAL(
        SalaryStructureDAL dal,
        ITenantService tenantService,
        ILogger<SalaryStructureBAL> logger)
    {
        _dal = dal;
        _tenantService = tenantService;
        _logger = logger;
    }

  
    public async Task<PagedResult<SalaryStructureDTO>> GetAll(int page, int size, string? search)
    {
        var orgId = _tenantService.GetOrgId();

        var (data, count) = await _dal.GetAll(page, size, search, orgId);

        // Convert entities to DTOs to prevent circular references
        var dtos = data.Select(s => MapToDTO(s)).ToList();

        return new PagedResult<SalaryStructureDTO>
        {
            Items = dtos,
            TotalCount = count,
            PageNumber = page,
            PageSize = size
        };
    }

    
    public async Task<SalaryStructureDTO> Create(SalaryStructureModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.StructureName))
            //throw new ArgumentException("Structure name is required.", nameof(model.StructureName));
            throw new ArgumentException(PmHrmsConstants.SalaryStructureMessages.StructureNameRequired, nameof(model.StructureName));

        if (!model.Components.Any())
            //throw new ArgumentException("At least one component is required.", nameof(model.Components));
            throw new ArgumentException(PmHrmsConstants.SalaryStructureMessages.ComponentRequired, nameof(model.Components));

        var orgId = _tenantService.GetOrgId();




        // Only one default structure per organization
        if (model.IsDefault)
            await _dal.RemoveExistingDefault(orgId);

              var masterIds = model.Components.Select(c => c.ComponentMasterId).Distinct().ToList();
              var masters   = await _dal.GetMastersByIds(masterIds);  
              var masterMap = masters.ToDictionary(m => m.Id, m => m.ComponentName);

        // Map DTO to domain entity
        var entity = new SalaryStructure
        {
            StructureName = model.StructureName.Trim(),
            OrganizationId = orgId,

            //PayType = model.PayType ?? "Monthly",
            PayType = model.PayType ?? PmHrmsConstants.SalaryStructureMessages.DefaultPayType,

            IsDefault = model.IsDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Components = model.Components
                .Where(c => c.ComponentMasterId > 0 && c.Amount >= 0) // Validation at BAL layer
                .Select(c => new SalaryComponent
                {
                    ComponentMasterId = c.ComponentMasterId,
                    ComponentName     = masterMap.GetValueOrDefault(c.ComponentMasterId)
                                    ?? c.ComponentName?.Trim()
                                    ?? string.Empty,
                    Amount = c.Amount,
                    IsEarning = c.IsEarning,
                    OrganizationId = orgId // Set org context for the component
                })
                .ToList()
        };

        var result = await _dal.Add(entity);
        return MapToDTO(result);
    }

    
    public async Task<SalaryStructureDTO?> Update(int id, SalaryStructureModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.StructureName))
            //throw new ArgumentException("Structure name is required.", nameof(model.StructureName));
            throw new ArgumentException(PmHrmsConstants.SalaryStructureMessages.StructureNameRequired, nameof(model.StructureName));

        if (!model.Components.Any())
            //throw new ArgumentException("At least one component is required.", nameof(model.Components));
            throw new ArgumentException(PmHrmsConstants.SalaryStructureMessages.ComponentRequired, nameof(model.Components));

        var entity = await _dal.GetById(id);
        if (entity == null)
            return null;

        // Only one default structure per organization
        if (model.IsDefault && !entity.IsDefault)
            await _dal.RemoveExistingDefault(entity.OrganizationId);

        // Update scalar properties
        entity.StructureName = model.StructureName.Trim();

        //entity.PayType = model.PayType ?? "Monthly";
        entity.PayType = model.PayType ?? PmHrmsConstants.SalaryStructureMessages.DefaultPayType;

        entity.IsDefault = model.IsDefault;
       

        var components = model.Components
            .Where(c => c.ComponentMasterId > 0 && c.Amount >= 0)
            .Select(c => new SalaryComponent
            {
                ComponentMasterId = c.ComponentMasterId,
                ComponentName = c.ComponentName?.Trim() ?? string.Empty,
                Amount = c.Amount,
                IsEarning = c.IsEarning,
                OrganizationId = entity.OrganizationId
            })
            .ToList();

        var result = await _dal.Update(entity, components);
        return result == null ? null : MapToDTO(result);
    }

    
    public async Task<bool> Delete(int id)
    {
        return await _dal.Delete(id);
    }

    
    public async Task AssignToDesignation(int designationId, int salaryStructureId)
    {
        var orgId = _tenantService.GetOrgId();
        await _dal.AssignToDesignation(designationId, salaryStructureId, orgId);
    }

    
    public async Task<List<DesignationSalaryMappingDTO>> GetDesignationMappings()
    {
        var data = await _dal.GetDesignationMappings();

        return data
            .Select(x => new DesignationSalaryMappingDTO
            {
                DesignationSalaryMappingId = x.Id,
                DesignationId = x.DesignationId,
                DesignationName = x.Designation.DesignationName,
                SalaryStructureId = x.SalaryStructureId,
                SalaryStructureName = x.SalaryStructure.StructureName
            })
            .ToList();
    }

   
    public async Task<List<SalaryComponentMaster>> LoadMaster()
    {
        try
        {
            _logger.LogInformation($"[SalaryStructure LoadMaster BAL] Loading salary component masters");
            
            var result = await _dal.LoadMaster();
            
            _logger.LogInformation($"[SalaryStructure LoadMaster BAL] Successfully loaded {result.Count} masters");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[SalaryStructure LoadMaster BAL] Error loading masters - {ex.Message}\\n Stack Trace: {ex?.StackTrace}");
            throw;
        }
    }

    
    public async Task<SalaryResult?> GetSalaryByEmployeeId(int employeeId)
    {
        var orgId = _tenantService.GetOrgId();

        var salary = await _dal.GetEmployeeSalary(employeeId, orgId);

        if (salary == null)
            return null;

        return new SalaryResult(salary, SalarySource.DEFAULT);
    }

    /// <summary>
    /// Maps SalaryStructure entity to DTO to prevent circular reference serialization issues
    /// </summary>
    private static SalaryStructureDTO MapToDTO(SalaryStructure entity)
    {
        return new SalaryStructureDTO
        {
            SalaryStructureId = entity.SalaryStructureId,
            OrganizationId = entity.OrganizationId,
            StructureName = entity.StructureName,
            PayType = entity.PayType,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            Components = entity.Components?
                .Select(c => new SalaryComponentDTO
                {
                    SalaryComponentId = c.SalaryComponentId,
                    SalaryStructureId = c.SalaryStructureId,
                    ComponentMasterId = c.ComponentMasterId,
                     ComponentName     = !string.IsNullOrWhiteSpace(c.SalaryComponentMaster?.ComponentName)
                                        ? c.SalaryComponentMaster.ComponentName
                                        : c.ComponentName,
                    Amount = c.Amount,
                    IsEarning = c.IsEarning
                })
                .ToList() ?? new List<SalaryComponentDTO>()
        };
    }
}