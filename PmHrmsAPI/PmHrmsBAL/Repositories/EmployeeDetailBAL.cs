using Azure;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class EmployeeDetailBAL : IEmployeeDetailBAL
    {
        private readonly EmployeeDetailDAL _detailDAL;
        private readonly IPermissionService _permissionService;
        private readonly IWorkPolicyBAL _workPolicyBAL;
        private readonly IEmployeeBAL _employeeBAL;

        public EmployeeDetailBAL(
        EmployeeDetailDAL detailDAL,
        IPermissionService permissionService,
        IWorkPolicyBAL workPolicyBAL,
        IEmployeeBAL employeeBAL   
    )
        {
            _detailDAL = detailDAL;
            _permissionService = permissionService;
            _workPolicyBAL = workPolicyBAL;
            _employeeBAL = employeeBAL;
        }



        public async Task<EmployeeDetailResponseModel?> GetDetailByEmployeeId(int employeeId)
        {
            _permissionService.Ensure(PermissionKeys.EMP_PROFILE_VIEW);

            var e = await _detailDAL.GetDetailByEmployeeId(employeeId);

            if (e != null)
            {
                return MapToResponse(e);
            }

            return new EmployeeDetailResponseModel
            {
                EmployeeId = employeeId
            };
        }

        public async Task<EmployeeDetailResponseModel?> AddOrUpdateDetail(EmployeeDetailModel request)
        {
            _permissionService.Ensure(PermissionKeys.EMP_EDIT_PERSONAL);

            var existing = await _detailDAL.GetDetailByEmployeeId(request.EmployeeId);

            if (existing == null)
            {
                var newEntity = new EmployeeDetail
                {
                    EmployeeId = request.EmployeeId,
                    DateOfBirth = request.DateOfBirth,
                    BloodGroup = request.BloodGroup,
                    MaritalStatus = request.MaritalStatus,
                    FatherName = request.FatherName,
                    PanNumber = request.PanNumber,
                    AadharNumber = request.AadharNumber,
                    PassportNumber = request.PassportNumber,
                    CurrentAddressLine = request.CurrentAddressLine,
                    CurrentCity = request.CurrentCity,
                    CurrentStateId = request.CurrentStateId,
                    CurrentCountryId = request.CurrentCountryId,
                    CurrentZipCode = request.CurrentZipCode,
                    LinkedinUrl = request.LinkedinUrl,
                    GithubUrl = request.GithubUrl
                };
                await _detailDAL.AddDetail(newEntity);
            }
            else
            {
                existing.DateOfBirth = request.DateOfBirth;
                existing.BloodGroup = request.BloodGroup;
                existing.MaritalStatus = request.MaritalStatus;
                existing.FatherName = request.FatherName;
                existing.PanNumber = request.PanNumber;
                existing.AadharNumber = request.AadharNumber;
                existing.PassportNumber = request.PassportNumber;
                existing.CurrentAddressLine = request.CurrentAddressLine;
                existing.CurrentCity = request.CurrentCity;
                existing.CurrentStateId = request.CurrentStateId;
                existing.CurrentCountryId = request.CurrentCountryId;
                existing.CurrentZipCode = request.CurrentZipCode;
                existing.LinkedinUrl = request.LinkedinUrl;
                existing.GithubUrl = request.GithubUrl;

                await _detailDAL.UpdateDetail(existing);
            }

            return await GetDetailByEmployeeId(request.EmployeeId);
        }

        private EmployeeDetailResponseModel MapToResponse(EmployeeDetail e)
        {
            return new EmployeeDetailResponseModel
            {
                DetailId = e.DetailId,
                EmployeeId = e.EmployeeId,
                DateOfBirth = e.DateOfBirth,
                BloodGroup = e.BloodGroup,
                MaritalStatus = e.MaritalStatus,
                FatherName = e.FatherName,
                PanNumber = e.PanNumber,
                AadharNumber = e.AadharNumber,
                PassportNumber = e.PassportNumber,
                CurrentAddressLine = e.CurrentAddressLine,
                CurrentCity = e.CurrentCity,
                CurrentZipCode = e.CurrentZipCode,
                LinkedinUrl = e.LinkedinUrl,
                GithubUrl = e.GithubUrl,
                CurrentStateId = e.CurrentStateId,
                CurrentStateName = e.CurrentState?.StateName,
                CurrentCountryId = e.CurrentCountryId,
                CurrentCountryName = e.CurrentCountry?.CountryName
            };
        }
    }
}