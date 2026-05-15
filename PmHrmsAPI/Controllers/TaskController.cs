using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.Models;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskController : ControllerBase
    {
        private readonly ITaskBAL _taskBAL;

        public TaskController(ITaskBAL taskBAL)
        {
            _taskBAL = taskBAL;
        }

        // ════════════════════════════════════════════════════
        // TASK CRUD
        // ════════════════════════════════════════════════════

        // GET api/task?pageNumber=1&pageSize=10&status=Pending&priority=3
        [HttpGet]
        public async Task<IActionResult> GetAllTasks(
            [FromQuery] int pageNumber    = 1,
            [FromQuery] int pageSize      = 10,
            [FromQuery] string? search    = null,
            [FromQuery] string? status    = null,
            [FromQuery] int? priority     = null)
        {
            try
            {
                var (items, totalCount) = await _taskBAL.GetAllTasks(
                    pageNumber, pageSize, search, status, priority);

                var paged = new PagedResult<TaskResponseModel>
                {
                    Items      = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize   = pageSize
                };

                return Ok(new ApiResponseModel<PagedResult<TaskResponseModel>>(
                    true, "Tasks retrieved successfully", paged));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving tasks.", ex.Message));
            }
        }

        // GET api/task/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTask(int id)
        {
            try
            {
                var task = await _taskBAL.GetTask(id);
                if (task == null)
                    return NotFound(new ApiResponseModel<string>(false, "Task not found.", null));

                return Ok(new ApiResponseModel<TaskResponseModel>(
                    true, "Task retrieved successfully", task));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving the task.", ex.Message));
            }
        }

        // POST api/task
        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] TaskModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid data.", null));

                var created = await _taskBAL.AddTask(model);

                return Ok(new ApiResponseModel<TaskResponseModel?>(
                    true, "Task created successfully. Assignments are being expanded in background.", created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while creating the task.", ex.Message));
            }
        }

        // PUT api/task/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid data.", null));

                var updated = await _taskBAL.UpdateTask(id, model);
                if (updated == null)
                    return NotFound(new ApiResponseModel<string>(false, "Task not found.", null));

                return Ok(new ApiResponseModel<TaskResponseModel?>(
                    true, "Task updated successfully", updated));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while updating the task.", ex.Message));
            }
        }

        // DELETE api/task/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var deleted = await _taskBAL.DeleteTask(id);
                if (!deleted)
                    return NotFound(new ApiResponseModel<string>(false, "Task not found.", null));

                return Ok(new ApiResponseModel<string>(true, "Task deleted successfully", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while deleting the task.", ex.Message));
            }
        }

        // ════════════════════════════════════════════════════
        // PROGRESS TRACKING
        // ════════════════════════════════════════════════════

        // GET api/task/5/progress?status=Pending&pageNumber=1
        [HttpGet("{id:int}/progress")]
        public async Task<IActionResult> GetTaskProgress(
            int id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 20,
            [FromQuery] string? status = null)
        {
            try
            {
                var (items, totalCount) = await _taskBAL.GetTaskProgress(id, pageNumber, pageSize, status);

                var paged = new PagedResult<TaskProgressResponseModel>
                {
                    Items      = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize   = pageSize
                };

                return Ok(new ApiResponseModel<PagedResult<TaskProgressResponseModel>>(
                    true, "Task progress retrieved successfully", paged));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving task progress.", ex.Message));
            }
        }

        // PATCH api/task/5/progress/me  — employee updates their own status
        [HttpPatch("{id:int}/progress/me")]
        public async Task<IActionResult> UpdateMyTaskStatus(int id, [FromBody] UpdateTaskStatusModel model)
        {
            try
            {
                
                if (model == null || !TaskStatuses.EmployeeWorkflow.Contains(model.Status))
                    return BadRequest(new ApiResponseModel<string>(
                        false, "Invalid employee task status. Allowed values are Pending, InProgress, ReviewRequested, or Completed. Completed is allowed only for self-review tasks.", null));

                var empIdClaim = User.FindFirst("EmployeeId") ;                              
                if (empIdClaim == null)
                    return Unauthorized(new ApiResponseModel<string>( 
                        false, "Invalid token: EmployeeId claim missing", null));                             
                                                                                
                int employeeId = int.Parse(empIdClaim.Value);

                var updated = await _taskBAL.UpdateMyTaskStatus(id, employeeId, model);
                if (!updated)
                    return BadRequest(new ApiResponseModel<string>(
                        false, "Task status could not be updated for the current workflow state.", null));

                return Ok(new ApiResponseModel<bool>(true, "Task status updated successfully", true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while updating task status.", ex.Message));
            }
        }

        // ════════════════════════════════════════════════════
        // FOLLOW-UPS
        // ════════════════════════════════════════════════════

        [HttpPatch("progress/{id:int}/review")]
        public async Task<IActionResult> UpdateTaskReviewStatus(int id, [FromBody] UpdateTaskReviewModel model)
        {
            try
            {
                if (model == null || !TaskStatuses.ReviewerWorkflow.Contains(model.Status))
                    return BadRequest(new ApiResponseModel<string>(
                        false, "Invalid review status. Allowed values are UnderReview or Completed.", null));

                var empIdClaim = User.FindFirst("EmployeeId");
                if (empIdClaim == null)
                    return Unauthorized(new ApiResponseModel<string>(
                        false, "Invalid token: EmployeeId claim missing", null));

                int reviewerEmployeeId = int.Parse(empIdClaim.Value);

                var updated = await _taskBAL.UpdateTaskReviewStatus(id, reviewerEmployeeId, model);
                if (!updated)
                    return BadRequest(new ApiResponseModel<string>(
                        false, "Review status could not be updated for the current workflow state.", null));

                return Ok(new ApiResponseModel<bool>(true, "Task review updated successfully", true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while updating task review.", ex.Message));
            }
        }

        // GET api/task/5/followups
        [HttpGet("{id:int}/followups")]
        public async Task<IActionResult> GetFollowUps(
            int id,
            [FromQuery] bool employeeContext = false,
            [FromQuery] bool markAsRead = false)
        {
            try
            {
                var followUps = await _taskBAL.GetFollowUps(id, employeeContext, markAsRead);

                return Ok(new ApiResponseModel<List<TaskFollowUpResponseModel>>(
                    true, "Follow-ups retrieved successfully", followUps));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving follow-ups.", ex.Message));
            }
        }

        // POST api/task/5/followups
        [HttpPost("{id:int}/followups")]
        public async Task<IActionResult> AddFollowUp(int id, [FromBody] TaskFollowUpModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new ApiResponseModel<string>(false, "Invalid data.", null));

                if (model.IsScheduled && model.ScheduledAt == null)
                    return BadRequest(new ApiResponseModel<string>(
                        false, "ScheduledAt is required when IsScheduled is true.", null));

                var created = await _taskBAL.AddFollowUp(id, model);

                var message = model.IsScheduled
                    ? $"Follow-up scheduled for {model.ScheduledAt:yyyy-MM-dd HH:mm} UTC"
                    : "Follow-up sent successfully";

                return Ok(new ApiResponseModel<TaskFollowUpResponseModel?>(true, message, created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while creating the follow-up.", ex.Message));
            }
        }



        // GET api/task/5/notes
        [HttpGet("{id:int}/notes")]
        public async Task<IActionResult> GetNotes(int id)
        {
            try
            {
                var notes = await _taskBAL.GetNotes(id);
                return Ok(new ApiResponseModel<List<TaskNoteResponseModel>>(
                    true, "Notes retrieved successfully", notes));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while retrieving notes.", ex.Message));
            }
        }

        // POST api/task/5/notes
        [HttpPost("{id:int}/notes")]
        public async Task<IActionResult> AddNote(int id, [FromBody] TaskNoteModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Content))
                    return BadRequest(new ApiResponseModel<string>(false, "Note content is required.", null));

                // Resolve author from JWT — cannot be spoofed
                var empIdClaim = User.FindFirst("EmployeeId");
                if (empIdClaim == null)
                    return Unauthorized(new ApiResponseModel<string>(
                        false, "Invalid token: EmployeeId claim missing", null));

                int createdByEmployeeId = int.Parse(empIdClaim.Value);
                var created = await _taskBAL.AddNote(id, createdByEmployeeId, model);

                return Ok(new ApiResponseModel<TaskNoteResponseModel?>(
                    true, "Note added successfully", created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseModel<string>(
                    false, "An error occurred while adding the note.", ex.Message));
            }
        }



        // ════════════════════════════════════════════════════
// MY TASKS (employee self-view)
// ════════════════════════════════════════════════════

// GET api/task/my?status=Pending&priority=3
[HttpGet("my")]
public async Task<IActionResult> GetMyTasks(
    [FromQuery] string? status   = null,
    [FromQuery] int? priority    = null)
{
    try
    {
        var empIdClaim = User.FindFirst("EmployeeId");
        if (empIdClaim == null)
            return Unauthorized(new ApiResponseModel<string>(
                false, "Invalid token: EmployeeId claim missing", null));

        int employeeId = int.Parse(empIdClaim.Value);

        var tasks = await _taskBAL.GetMyTasks(employeeId, status, priority);

        return Ok(new ApiResponseModel<List<MyTaskResponseModel>>(
            true, "My tasks retrieved successfully", tasks));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponseModel<string>(
            false, "An error occurred while retrieving your tasks.", ex.Message));
    }
}

// ════════════════════════════════════════════════════
// REVIEWING TASKS (reviewer view)
// ════════════════════════════════════════════════════

// GET api/task/reviewing
[HttpGet("reviewing")]
public async Task<IActionResult> GetReviewingTasks()
{
    try
    {
        var empIdClaim = User.FindFirst("EmployeeId");
        if (empIdClaim == null)
            return Unauthorized(new ApiResponseModel<string>(
                false, "Invalid token: EmployeeId claim missing", null));

        int employeeId = int.Parse(empIdClaim.Value);

        var tasks = await _taskBAL.GetReviewingTasks(employeeId);

        return Ok(new ApiResponseModel<List<ReviewingTaskResponseModel>>(
            true, "Reviewing tasks retrieved successfully", tasks));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiResponseModel<string>(
            false, "An error occurred while retrieving reviewing tasks.", ex.Message));
    }
}



    }
}
