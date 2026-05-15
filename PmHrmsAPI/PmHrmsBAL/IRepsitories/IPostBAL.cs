using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
     public interface IPostBAL
    {
        Task<(List<PostResponseModel> Items, int TotalCount)> GetAllPosts(
            int pageNumber, int pageSize, string? searchTerm, string? postType, bool isAdmin = false);
        Task<PostResponseModel?> GetPost(int postId);
        Task<PostResponseModel?> AddPost(PostModel model);
        Task<PostResponseModel?> UpdatePost(int postId, PostModel model);
        Task<bool> DeletePost(int postId);
        Task<bool> TogglePublish(int postId, bool publish);
    }
}
