using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class DepartmentBAL : IDepartmentBAL
    {
        private readonly DepartmentDAL _departmentDAL;
        private readonly IPermissionService _permissionService;

        public DepartmentBAL(
            DepartmentDAL departmentDAL,
            IPermissionService permissionService)

        {

            _departmentDAL = departmentDAL;
            _permissionService = permissionService;
        }


        public async Task<PagedResult<DepartmentResponseModel>> GetAllDepartments(int page, int size, string? search)

        {
            _permissionService.Ensure(PermissionKeys.DEPT_VIEW);

            var (entities, count) =
                await _departmentDAL.GetAllDepartments(page, size, search);

            var items = entities.Select(d => new DepartmentResponseModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                HeadOfDepartmentId = d.HeadOfDepartmentId,
                OrganizationId = d.OrganizationId,
                OrganizationName = d.Organization?.OrganizationName,
                EmployeeCount = d.Employees.Count(),
                Designations = d.Designations.Select(des => new DesignationResponseModel
                {
                    DesignationId = des.DesignationId,
                    DesignationName = des.DesignationName,
                    HierarchyLevel = des.HierarchyLevel,
                    IsSystemDefault = des.IsSystemDefault
                }).ToList()
            }).ToList();

            return new PagedResult<DepartmentResponseModel>
            {
                Items = items,
                TotalCount = count,
                PageNumber = page,
                PageSize = size
            };
        }

      
        public async Task<DepartmentResponseModel?> GetDepartment(int id)
        {
            _permissionService.Ensure(PermissionKeys.DEPT_VIEW);

            var d = await _departmentDAL.GetDepartment(id);
            if (d == null) return null;

            return new DepartmentResponseModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                HeadOfDepartmentId = d.HeadOfDepartmentId,
                OrganizationId = d.OrganizationId,
                OrganizationName = d.Organization?.OrganizationName
            };
        }

       
        public async Task<DepartmentResponseModel?> AddDepartment(
            DepartmentModel request,
            int orgId,
            int loggedInEmployeeId)
        {
            _permissionService.Ensure(PermissionKeys.DEPT_CREATE);

            var departmentName = request.DepartmentName.Trim();

            var exists = await _departmentDAL.DepartmentExists(departmentName, orgId);
            if (exists)
                //throw new ApplicationException("Department already exists");
                throw new ApplicationException(PmHrmsConstants.DepartmentMessages.DuplicateDepartment);


            var entity = new Department
            {
                DepartmentName = departmentName,
                HeadOfDepartmentId = loggedInEmployeeId,
                OrganizationId = orgId,
                IsActive = true,
                IsSystemDefault = false
            };

            var result = await _departmentDAL.AddDepartment(entity);

            return await GetDepartment(result.DepartmentId);
        }

       
        public async Task<DepartmentResponseModel?> UpdateDepartment(
            int id,
            DepartmentModel request)
        {

            _permissionService.Ensure(PermissionKeys.DEPT_EDIT);

            var departmentName = request.DepartmentName.Trim();
            
            var exists = await _departmentDAL.DepartmentExistsForUpdate(
                departmentName,
                request.OrganizationId,
                id);

            if (exists)
                //throw new ApplicationException("Department already exists.");
                throw new ApplicationException(PmHrmsConstants.DepartmentMessages.DuplicateDepartment);


            var entity = new Department
            {
                DepartmentId = id,
                DepartmentName = departmentName,
                HeadOfDepartmentId = request.HeadOfDepartmentId,
                OrganizationId = request.OrganizationId
            };

            var result = await _departmentDAL.UpdateDepartment(entity);
            if (result == null) return null;

            return await GetDepartment(result.DepartmentId);
        }

     
        public async Task<bool> DeleteDepartment(int id)
        {
            _permissionService.Ensure(PermissionKeys.DEPT_DELETE);
            return await _departmentDAL.DeleteDepartment(id);
        }
    }
}
