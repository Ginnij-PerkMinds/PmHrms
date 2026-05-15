using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using Hangfire;
using PmHrmsAPI.PmHrmsBAL.Jobs;

namespace PmHrmsAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
     public class OrganizationController : ControllerBase   
    {
        private readonly IOrganizationBAL _organizationBAL;
        public OrganizationController(IOrganizationBAL organizationBAL)
        {
            _organizationBAL = organizationBAL;    
        }

                             
        [HttpGet]
        public async Task<IActionResult> GetAllOrganization([FromQuery] int pageNumber = 1,  [FromQuery] int pageSize = 10,   [FromQuery] string? searchTerm = null)
        {
        

            try
            {
                var (organizations, totalCount) = await _organizationBAL.GetAllOrganization( pageNumber , pageSize , searchTerm);

                var pagedData = new PagedResult<OrganizationResponseModel>
                {
                    Items = organizations,  
                    TotalCount = totalCount,  
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };



                return Ok(new ApiResponseModel<PagedResult<OrganizationResponseModel>>(
                            true,
                            "Organizations retrieved successfully",
                            pagedData
                        )
                {
                    
                });
            }

            catch (Exception ex)
            {
                return StatusCode(500,
                  new ApiResponseModel<string>(
                  false,
                  "An error occurred while retrieving organizations.",
                  ex.Message));
            }


        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrganization(int id)
        {
            try
            {
                var organization = await _organizationBAL.GetOrganization(id);
                if (organization == null)
                {
                    return NotFound(
                      new ApiResponseModel<string>(
                      false,
                      "Organization not found.",
                      null));
                }



                return Ok(
                  new ApiResponseModel<OrganizationResponseModel?>(
                  true,
                  "Organization retrieved successfully",
                  organization));

            }

            catch (Exception ex)
            {
                return StatusCode(500,
                  new ApiResponseModel<string>(
                  false,
                  "An error occurred while retrieving the organization.",
                  ex.Message));
            }


        }



        [HttpPost]
        public async Task<IActionResult> AddOrganization([FromBody] OrganizationModel organization)
        {
            try
            {

                if (organization == null)
                {
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid Data.", null));
                }

                var addedOrganization = await _organizationBAL.AddOrganization(organization);


                return Ok(
                  new ApiResponseModel<OrganizationResponseModel?>(
                  true,
                  "Organization added successfully",
                  addedOrganization));

            }
            catch (Exception ex)
            {
                return StatusCode(500,
                  new ApiResponseModel<string>(
                  false,
                  "An error occurred while adding the organization.",
                  ex.Message));
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("setup/status")]
        public async Task<IActionResult> GetSetupStatus()
        {
            try
            {
                var orgIdClaim = User.FindFirst("OrgId");

                if (orgIdClaim == null)
                {
                    return Unauthorized(new ApiResponseModel<string>(false, "Invalid Token: OrgId not found", null));
                }

                int orgId = int.Parse(orgIdClaim.Value);
                var status = await _organizationBAL.GetOrgSetupStatus(orgId);

                return Ok(new ApiResponseModel<OrgSetupStatusResponse>(
                    true,
                    "Organization setup status",    
                    status             
                ));
            }
            catch (Exception ex)   
            {
                return StatusCode(500,   
                    new ApiResponseModel<string>(false, "Failed to get setup status", ex.Message));
            }
        }





        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var orgIdClaim = User.FindFirst("OrgId");
                if (orgIdClaim == null)
                {
                    return Unauthorized(new ApiResponseModel<string>(false, "Invalid Token: OrgId not found", null));
                }

                int orgId = int.Parse(orgIdClaim.Value);
                var stats = await _organizationBAL.GetDashboardStats(orgId);

                return Ok(new ApiResponseModel<DashboardStatsResponse>(
                    true,
                    "Dashboard stats retrieved successfully",
                    stats
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, 
                    "An error occurred while retrieving dashboard stats.", 
                    ex.Message));
            }
        }
        [HttpPost("{id:int}/logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(
         int id,
         [FromForm] UploadLogoRequest request)
            {
            var file = request.File;
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseModel<string>(false, "Logo file is required", null));
                }

                var uploadedPath = await _organizationBAL.UploadOrganizationLogo(id, file);
                if (uploadedPath == null)
                {
                    return NotFound(new ApiResponseModel<string>(false, "Organization not found", null));
                }

                return Ok(new ApiResponseModel<string>(true, "Logo uploaded successfully", uploadedPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new ApiResponseModel<string>(false, "Failed to upload organization logo", ex.Message));
            }                 
        }

        [HttpDelete("{id:int}/logo")]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            try
            {
                var deleted = await _organizationBAL.DeleteOrganizationLogo(id);
                if (!deleted)
                {
                    return NotFound(new ApiResponseModel<string>(false, "Organization not found", null));
                }

                return Ok(new ApiResponseModel<bool>(true, "Logo deleted successfully", true));
            }
            catch (Exception ex)
            {   
                return StatusCode(500,
                    new ApiResponseModel<string>(false, "Failed to delete organization logo", ex.Message));
            }                         
        }

        



        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] OrganizationModel organization)
        {
            try
            {
                if (organization == null)
                {
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid Data.", null));
                }

                var updatedOrganization = await _organizationBAL.UpdateOrganization(id, organization);

                if (updatedOrganization == null)
                {
                    return NotFound(
                      new ApiResponseModel<string>(
                      false,
                      "Organization not found.",
                      null));
                }
                return Ok(
                  new ApiResponseModel<OrganizationResponseModel?>(
                  true,
                  "Organization updated successfully",
                  updatedOrganization));
            }


            catch (Exception ex)
            {
                return StatusCode(500,
                  new ApiResponseModel<string>(
                  false,
                  "An error occurred while updating the organization.",
                  ex.Message));
            }
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            try
            {
                var isDeleted = await _organizationBAL.DeleteOrganization(id);
                if (!isDeleted)
                {
                    return NotFound(
                      new ApiResponseModel<string>(
                      false,
                      "Organization not found.",
                      null));
                }
                return Ok(
                  new ApiResponseModel<string>(
                  true,
                  "Organization deleted successfully",
                  null));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                  new ApiResponseModel<string>(
                  false,
                  "An error occurred while deleting the organization.",
                  ex.Message));
            }

        }


        [HttpPost("run-holiday-job")]
            public IActionResult RunHolidayJob(
                [FromServices] IBackgroundJobClient client,
                [FromQuery] int? year = null)  
            {
                var targetYear = year ?? DateTime.Now.Year + 1;
                client.Enqueue<HolidayAutomationJob>(x => x.RunAsync(targetYear));
                return Ok(new { message = $"Holiday job triggered for year {targetYear}" });
            }

    }

}
