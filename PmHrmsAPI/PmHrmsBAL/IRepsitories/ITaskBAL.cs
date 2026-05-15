 
 using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
 public interface ITaskBAL
    {
        
        Task<(List<TaskResponseModel> Items, int TotalCount)> GetAllTasks(int pageNumber, int pageSize, string? searchTerm, string? status, int? priority);
        Task<TaskResponseModel?> GetTask(int taskId);
        Task<TaskResponseModel?> AddTask(TaskModel model);
        Task<TaskResponseModel?> UpdateTask(int taskId, TaskModel model);
        Task<bool> DeleteTask(int taskId);
 
       
        Task<(List<TaskProgressResponseModel> Items, int TotalCount)> GetTaskProgress(int taskId, int pageNumber, int pageSize, string? status);
        Task<bool> UpdateMyTaskStatus(int taskId, int employeeId, UpdateTaskStatusModel model);
        Task<bool> UpdateTaskReviewStatus(int taskId, int reviewerEmployeeId, UpdateTaskReviewModel model);
 
        
        Task<List<TaskFollowUpResponseModel>> GetFollowUps(int taskId, bool employeeContext = false, bool markAsRead = false);
        Task<TaskFollowUpResponseModel?> AddFollowUp(int taskId, TaskFollowUpModel model);

        Task<List<TaskNoteResponseModel>> GetNotes(int taskId);
        Task<TaskNoteResponseModel?> AddNote(int taskId, int createdByEmployeeId, TaskNoteModel model);

        Task<List<MyTaskResponseModel>> GetMyTasks( int employeeId, string? status, int? priority);

        Task<List<ReviewingTaskResponseModel>> GetReviewingTasks(int employeeId);  
        Task ExpandAssignmentsAsync(int taskId, int orgId);
        Task SendFollowUpAsync(int followUpId, int orgId);
    }
}
