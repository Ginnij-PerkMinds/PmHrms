using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostController : ControllerBase
    {
        private readonly IPostBAL _postBAL;

        public PostController(IPostBAL postBAL)
        {
            _postBAL = postBAL;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? postType = null,
            [FromQuery] bool isAdmin = false)
        {
            try
            {
                if (User.IsInRole("SuperAdmin") || User.IsInRole("HR"))
                {
                    isAdmin = true;
                }

                var (items, totalCount) = await _postBAL.GetAllPosts(pageNumber, pageSize, search, postType, isAdmin);

                var paged = new PagedResult<PostResponseModel>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(new ApiResponseModel<PagedResult<PostResponseModel>>(true, "Posts retrieved successfully", paged));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(false, "Error", ex.Message));
            }
        }

        // ── GET ONE ──────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPost(int id)
        {
            try
            {
                var post = await _postBAL.GetPost(id);
                if (post == null)
                    return NotFound(new ApiResponseModel<string>(false, "Post not found.", null));

                return Ok(new ApiResponseModel<PostResponseModel>(
                    true, "Post retrieved successfully", post));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving the post.", ex.Message));
            }
        }

        // ── CREATE ───────────────────────────────────────────────────
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPost([FromForm] PostModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid data.", null));

                var created = await _postBAL.AddPost(model);

                return Ok(new ApiResponseModel<PostResponseModel?>(
                    true, "Post created successfully", created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while creating the post.", ex.Message));
            }
        }

        // ── UPDATE ───────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] PostModel model) 
        {
            try
            {
                if (model == null)
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid data.", null));

                var updated = await _postBAL.UpdatePost(id, model);
                if (updated == null)
                    return NotFound(new ApiResponseModel<string>(false, "Post not found.", null));

                return Ok(new ApiResponseModel<PostResponseModel?>(
                    true, "Post updated successfully", updated));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while updating the post.", ex.Message));
            }
        }

        // ── TOGGLE PUBLISH ───────────────────────────────────────────
        [HttpPatch("{id:int}/publish")]
        public async Task<IActionResult> TogglePublish(int id, [FromQuery] bool publish)
        {
            try
            {
                var result = await _postBAL.TogglePublish(id, publish);
                if (!result)
                    return NotFound(new ApiResponseModel<string>(false, "Post not found.", null));

                var msg = publish ? "Post published successfully" : "Post unpublished successfully";
                return Ok(new ApiResponseModel<bool>(true, msg, true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while updating publish status.", ex.Message));
            }
        }

        // ── DELETE ───────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var deleted = await _postBAL.DeletePost(id);
                if (!deleted)
                    return NotFound(new ApiResponseModel<string>(false, "Post not found.", null));

                return Ok(new ApiResponseModel<string>(true, "Post deleted successfully", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while deleting the post.", ex.Message));
            }
        }
    }
}