using DocumentFormat.OpenXml.Vml.Office;
using PmHrmsAPI.PmHrmsBAL.IRepsitories;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsDAL.Utility;
using PmHrmsAPI.PmHrmsFAL.IRespositories;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Repositories
{
    public class PostBAL : IPostBAL
    {
        private readonly PostDAL _postDAL;
        private readonly IPermissionService _permissionService;
        private readonly ITenantService _currentUser;
        private readonly IImageFAL _imageFAL;

        public PostBAL(
            PostDAL postDAL,
            IPermissionService permissionService,
            ITenantService currentUser,
            IImageFAL imageFAL)
        {
            _postDAL           = postDAL;
            _permissionService = permissionService;
            _currentUser       = currentUser;
            _imageFAL = imageFAL;
        }

        // ── GET ALL ──────────────────────────────────────────────────
        public async Task<(List<PostResponseModel> Items, int TotalCount)> GetAllPosts(
            int pageNumber, int pageSize, string? searchTerm, string? postType, bool isAdmin = false)
        {
            _permissionService.Ensure(PermissionKeys.POST_VIEW);

            int orgid = _currentUser.GetOrgId();
            int? deptId = _currentUser.GetDepartmentId();
            int empId = _currentUser.GetCurrentUserID();

            //bool isAdmin = _currentUser.GetUserRole() == "SuperAdmin" || _currentUser.GetUserRole() == "HR";

            var (entities, total) = await _postDAL.GetAllPosts(
                orgid, pageNumber, pageSize, searchTerm, postType, deptId, empId, isAdmin);

            var items = new List<PostResponseModel>();
            foreach (var e in entities)
            {
                var taskCount = await _postDAL.GetLinkedTaskCount(e.Id);
                items.Add(MapToResponse(e, taskCount));
            }
            return (items, total);
        }

        // ── GET ONE ──────────────────────────────────────────────────
        public async Task<PostResponseModel?> GetPost(int postId)
        {
            _permissionService.Ensure(PermissionKeys.POST_VIEW);

               int orgid = _currentUser.GetOrgId();
            var entity = await _postDAL.GetPost(postId, orgid);
            if (entity == null) return null;

            var taskCount = await _postDAL.GetLinkedTaskCount(postId);
            return MapToResponse(entity, taskCount);
        }

        // ── ADD ──────────────────────────────────────────────────────
        public async Task<PostResponseModel?> AddPost(PostModel model)
        {
            _permissionService.Ensure(PermissionKeys.POST_CREATE);

            int orgid = _currentUser.GetOrgId();
            int empid = _currentUser.GetCurrentUserID();

            string? savedImagePath = null;
            if (model.ImageFile != null)
            {
                //savedImagePath = await _imageFAL.UploadImageAsync(model.ImageFile, "PostImagesPath");
                savedImagePath = await _imageFAL.UploadImageAsync(model.ImageFile, PmHrmsConstants.FolderNames.PostImages);
            }

            var entity = new Post
            {
                OrgId           = orgid,
                Title           = model.Title,
                Description     = model.Description,
                PostType        = model.PostType,
                IsPublished     = model.IsPublished,
                CreatedByUserId = empid,
                IsDeleted       = false,
                ImagePath = savedImagePath,
                VisibleFrom = model.VisibleFrom,
                VisibleUntil = model.VisibleUntil,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow,

                
                Targets = model.Targets.Select(t => new PostTarget
                {
                    OrgId      = orgid,
                    TargetType = t.TargetType,
                    TargetId   = t.TargetId
                }).ToList()
            };

            var created = await _postDAL.AddPost(entity);

            
            return await GetPost(created.Id);
        }

        // ── UPDATE ───────────────────────────────────────────────────
        public async Task<PostResponseModel?> UpdatePost(int postId, PostModel model)
        {
            _permissionService.Ensure(PermissionKeys.POST_EDIT);
            int orgId = _currentUser.GetOrgId();

            var existingPost = await _postDAL.GetPost(postId, orgId);
            if (existingPost == null) return null;

            if (model.ImageFile != null)
            {
                // Delete old/previous path
                // if (!string.IsNullOrEmpty(existingPost.ImagePath)) await _imageFAL.DeleteImageAsync(existingPost.ImagePath);

                //existingPost.ImagePath = await _imageFAL.UploadImageAsync(model.ImageFile, "PostImagesPath");
                existingPost.ImagePath = await _imageFAL.UploadImageAsync(model.ImageFile, PmHrmsConstants.FolderNames.PostImages);
            }

            existingPost.Title = model.Title;
            existingPost.Description = model.Description;
            existingPost.PostType = model.PostType;
            existingPost.IsPublished = model.IsPublished;
            existingPost.VisibleFrom = model.VisibleFrom;
            existingPost.VisibleUntil = model.VisibleUntil;
            existingPost.UpdatedAt = DateTime.UtcNow;

            existingPost.Targets = model.Targets.Select(t => new PostTarget
            {
                OrgId = orgId,
                PostId = postId,
                TargetType = t.TargetType,
                TargetId = t.TargetId
            }).ToList();

            var result = await _postDAL.UpdatePost(existingPost);
            return await GetPost(postId);
        }

        // ── TOGGLE PUBLISH ───────────────────────────────────────────
        public async Task<bool> TogglePublish(int postId, bool publish)
        {
            _permissionService.Ensure(PermissionKeys.POST_EDIT);
            int orgid = _currentUser.GetOrgId();    
            return await _postDAL.TogglePublish(postId, orgid, publish);
        }

        // ── DELETE ───────────────────────────────────────────────────
        public async Task<bool> DeletePost(int postId)
        {
            _permissionService.Ensure(PermissionKeys.POST_DELETE);
            int orgid = _currentUser.GetOrgId();
            int empid = _currentUser.GetCurrentUserID();
            return await _postDAL.DeletePost(postId, orgid);
        }

        // ── MAPPER ───────────────────────────────────────────────────
        private static PostResponseModel MapToResponse(Post e, int taskCount) => new()
        {
           
            PostId      = e.Id,
            OrgId       = e.OrgId,
            Title       = e.Title,
            Description = e.Description,
            PostType    = e.PostType,
            IsPublished = e.IsPublished,
            IsDeleted   = e.IsDeleted,
            ImagePath = e.ImagePath,
            VisibleFrom = e.VisibleFrom,
            VisibleUntil = e.VisibleUntil,


            CreatedByName = e.CreatedByEmployee != null
                ? $"{e.CreatedByEmployee.FirstName} {e.CreatedByEmployee.LastName}".Trim()
                : string.Empty,

            CreatedAt       = e.CreatedAt,
            UpdatedAt       = e.UpdatedAt,
            LinkedTaskCount = taskCount,

           
            Targets = e.Targets.Select(t => new PostTargetResponseModel
            {
                
                PostTargetId = t.Id,
                TargetType   = t.TargetType,
                TargetId     = t.TargetId,
                //TargetName   = t.TargetType == "All" ? "All Employees" : null
                TargetName = t.TargetType == PmHrmsConstants.PostMessages.All ? PostMessages.AllEmployees : null
            }).ToList()
        };
    }
}