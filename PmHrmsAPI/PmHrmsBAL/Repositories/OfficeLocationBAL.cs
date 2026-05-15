using Microsoft.Extensions.Logging;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

public class OfficeLocationBAL : IOfficeLocationBAL
{
    private readonly OfficeLocationDAL _dal;
    private readonly IPermissionService _permission;
    private readonly ILogger<OfficeLocationBAL> _logger;

    public OfficeLocationBAL(
        OfficeLocationDAL dal,
        IPermissionService permission,
        ILogger<OfficeLocationBAL> logger)
    {
        _dal = dal;
        _permission = permission;
        _logger = logger;
    }

    public async Task<List<OfficeLocation>> GetLocations(int orgId)
    {
        try
        {
            _logger.LogInformation("Checking ORG_VIEW permission for OrgId: {OrgId}", orgId);
            _permission.Ensure(PermissionKeys.ORG_VIEW);

            return await _dal.GetByOrgIdAsync(orgId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BAL while fetching office locations for OrgId: {OrgId}", orgId);
            throw;
        }
    }

    public async Task<OfficeLocation> AddLocation(int orgId, OfficeLocation model)
    {
        _permission.Ensure(PermissionKeys.ORG_EDIT);

        model.OrganizationId = orgId;

       
        if (model.IsDefault)
        {
            await _dal.SetDefaultLocationAsync(0, orgId); 
        }

        return await _dal.AddAsync(model);
    }

    public async Task<OfficeLocation?> UpdateLocation(int id, OfficeLocation model)
    {
        try
        {
            _permission.Ensure(PermissionKeys.ORG_EDIT);

            return await _dal.UpdateAsync(id, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating office location Id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> SetDefaultLocation(int locationId, int orgId)
    {
        _permission.Ensure(PermissionKeys.ORG_EDIT);

        return await _dal.SetDefaultLocationAsync(locationId, orgId);
    }



    public async Task<bool> DeleteLocation(int id)
    {
        try
        {
            _permission.Ensure(PermissionKeys.ORG_EDIT);
            return await _dal.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BAL while deleting office location Id: {Id}", id);
            throw;
        }
    }
}