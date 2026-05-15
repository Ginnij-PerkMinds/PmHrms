using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL
{
    public class DesignationBAL : IDesignationBAL
    {
        private readonly DesignationDAL _designationDAL;
        private readonly IPermissionService _permissionService;

        public DesignationBAL(
            DesignationDAL designationDAL,
            IPermissionService permissionService)
        {
            _designationDAL = designationDAL;
            _permissionService = permissionService;
        }

        public async Task<PagedResult<DesignationResponseModel>> GetAllDesignations(int page, int size, string? search, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.DESIG_VIEW);

            var (entities, count) = await _designationDAL.GetAllDesignations(page, size, search, orgId);


            var items = new List<DesignationResponseModel>();

            foreach (var d in entities)
            {
                var mapping = await _designationDAL
    .GetWorkPolicyByDesignationId(d.DesignationId, orgId);

                items.Add(new DesignationResponseModel
                {
                    DesignationId = d.DesignationId,
                    DesignationName = d.DesignationName,
                    HierarchyLevel = d.HierarchyLevel,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department?.DepartmentName,
                    OrganizationName = d.Department?.Organization?.OrganizationName,
                    WorkPolicyName = mapping 
                });
            }

            return new PagedResult<DesignationResponseModel>
            {
                Items = items,
                TotalCount = count,
                PageNumber = page,
                PageSize = size
            };
        }







        public async Task<List<DesignationResponseModel>>
            GetDesignationsByDepartment(int departmentId, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.DESIG_VIEW);

            var list = await _designationDAL.GetByDepartment(departmentId, orgId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<DesignationResponseModel?> GetDesignation(int id, int orgId)
        {
            _permissionService.Ensure(PermissionKeys.DESIG_VIEW);

            var d = await _designationDAL.GetDesignation(id, orgId);
            if (d == null) return null;

            return MapToResponse(d);
        }




        public async Task<DesignationResponseModel?> AddDesignation(
            DesignationModel request,
            int orgId,
            int loggedInEmployeeId)
        {

          
            _permissionService.Ensure(PermissionKeys.DESIG_CREATE);

            
            _permissionService.Ensure(PermissionKeys.DESIG_MANAGE);

            var name = request.DesignationName.Trim();

            if (request.DepartmentId <= 0)
                //throw new ApplicationException("Department is required");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DepartmentRequired);

            if (request.HierarchyLevel == null || request.HierarchyLevel < 1)
                //throw new ApplicationException("Hierarchy level must be greater than 0");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.HierarchyLevelInvalid);


            var isValid =
                await _designationDAL.IsDepartmentBelongsToOrg(
                    request.DepartmentId, orgId);

            if (!isValid)
                //throw new ApplicationException("Department does not belong to this organization");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DepartmentNotBelongOrg);

            var nameExists =
                await _designationDAL.DesignationExists(
                    name,
                    request.DepartmentId);

            if (nameExists)
                //throw new ApplicationException("Designation already exists in this department");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DuplicateDesignation);


            var hierarchyExists =
                await _designationDAL.HierarchyExists(
                    request.DepartmentId,
                    request.HierarchyLevel.Value);

            if (hierarchyExists)
                //throw new ApplicationException("Hierarchy level already exists in this department");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DuplicateHierarchy);

            var entity = new Designation
            {
                DesignationName = name,
                DepartmentId = request.DepartmentId,
                HierarchyLevel = request.HierarchyLevel.Value,
                IsActive = true,
                IsSystemDefault = false
            };

            var result = await _designationDAL.AddDesignation(entity);
            return await GetDesignation(result.DesignationId, orgId);
        }


        public async Task<DesignationResponseModel?> UpdateDesignation(
            int id,
            DesignationModel request,
            int orgId)
        {
            _permissionService.Ensure(PermissionKeys.DESIG_EDIT);


            var existing =
                await _designationDAL.GetDesignation(id, orgId);

            if (existing == null)
                return null;

            var name = request.DesignationName.Trim();

           
            var duplicateName =
                await _designationDAL.DesignationExistsForUpdate(
                    name,
                    request.DepartmentId,
                    id);


            if (duplicateName)
                //throw new ApplicationException("Designation already exists in this department");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DuplicateDesignation);


            var duplicateHierarchy =
                await _designationDAL.HierarchyExistsForUpdate(
                    request.DepartmentId,
                    request.HierarchyLevel ?? 1,
                    id);

            if (duplicateHierarchy)
                //throw new ApplicationException("Hierarchy level already exists in this department");
                throw new ApplicationException(PmHrmsConstants.DesignationMessages.DuplicateHierarchy);

            existing.DesignationName = name;
            existing.HierarchyLevel = request.HierarchyLevel ?? 1;
            existing.DepartmentId = request.DepartmentId;

            var result = await _designationDAL.UpdateDesignation(existing);
            return await GetDesignation(result!.DesignationId, orgId);
        }

        public async Task<bool> DeleteDesignation(int id)
        {
            _permissionService.Ensure(PermissionKeys.DESIG_DELETE);
            return await _designationDAL.DeleteDesignation(id);
        }

        private DesignationResponseModel MapToResponse(Designation d)
        {
            return new DesignationResponseModel
            {
                DesignationId = d.DesignationId,
                DesignationName = d.DesignationName,
                HierarchyLevel = d.HierarchyLevel,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department?.DepartmentName,
                OrganizationName = d.Department?.Organization?.OrganizationName
            };
        }
    }
}