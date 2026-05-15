namespace PmHrmsAPI.Controllers
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PmHrmsAPI.PmHrmsBAL.IRepsitories;
    using PmHrmsAPI.PmHrmsDAL.DbEntities;

    [Route("api/organization/{orgId:int}/office-location")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OfficeLocationController : ControllerBase
    {
        private readonly IOfficeLocationBAL _bal;
        private readonly ILogger<OfficeLocationController> _logger;

        public OfficeLocationController(
            IOfficeLocationBAL bal,
            ILogger<OfficeLocationController> logger)
        {
            _bal = bal;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int orgId)
        {
            try
            {
                _logger.LogInformation("API GET OfficeLocation called for OrgId: {OrgId}", orgId);

                var data = await _bal.GetLocations(orgId);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API error while fetching office locations for OrgId: {OrgId}", orgId);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] OfficeLocation model)
        {
            var result = await _bal.UpdateLocation(id, model);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("set-default/{id:int}")]
        public async Task<IActionResult> SetDefault(int orgId, int id)
        {
            var result = await _bal.SetDefaultLocation(id, orgId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int orgId, [FromBody] OfficeLocation model)
        {
            _logger.LogInformation("adding this model" , model);
            var result = await _bal.AddLocation(orgId, model);
            _logger.LogInformation("this is the result in add controller " , result);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bal.DeleteLocation(id);
            return Ok(deleted);
        }
    }
}
