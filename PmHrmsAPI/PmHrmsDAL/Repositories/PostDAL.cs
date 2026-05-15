using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;

namespace PmHrmsAPI.PmHrmsDAL.Repositories
{
    public class PostDAL
    {
        private readonly PmHrmsContext _context;

        public PostDAL(PmHrmsContext context)
        {
            _context = context;
        }

        // ── GET ALL ──────────────────────────────────────────────────
        public async Task<(List<Post> Items, int TotalCount)> GetAllPosts(
    int orgId, int pageNumber, int pageSize, string? searchTerm, string? postType,
    int? deptId = null, int? empId = null, bool isAdmin = false)
        {
            var query = _context.Posts
                .Where(p => p.OrgId == orgId && !p.IsDeleted)
                .Include(p => p.CreatedByEmployee)
                .Include(p => p.Targets)
                .AsNoTracking();

            if (!isAdmin)
            {
                var today = DateTime.UtcNow.Date;

                query = query.Where(p => (p.VisibleFrom == null || p.VisibleFrom.Value.Date <= today) &&
                                         (p.VisibleUntil == null || p.VisibleUntil.Value.Date >= today));

                if (deptId.HasValue || empId.HasValue)
                {
                    query = query.Where(p => p.Targets.Any(t =>
                        t.TargetType == "All" ||
                        (t.TargetType == "Department" && t.TargetId == deptId) ||
                        (t.TargetType == "Employee" && t.TargetId == empId)
                    ));
                }
            }
            else
            {
                Console.WriteLine("DEBUG: Admin Bypass Active. Fetching all posts for Org: " + orgId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Title.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));

            if (!string.IsNullOrWhiteSpace(postType))
                query = query.Where(p => p.PostType == postType);

            int totalCount = await query.CountAsync();
            var data = await query.OrderByDescending(p => p.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalCount);
        }

        // ── GET ONE ──────────────────────────────────────────────────
        public async Task<Post?> GetPost(int postId, int orgId)
        {
            return await _context.Posts
                .Where(p => p.Id == postId && p.OrgId == orgId && !p.IsDeleted)
                .Include(p => p.CreatedByEmployee)
                .Include(p => p.Targets)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // ── ADD ──────────────────────────────────────────────────────
        public async Task<Post> AddPost(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }

        // ── UPDATE ───────────────────────────────────────────────────
        public async Task<Post?> UpdatePost(Post post)
        {
            var existing = await _context.Posts
                .Include(p => p.Targets)
                .FirstOrDefaultAsync(p => p.Id == post.Id && p.OrgId == post.OrgId && !p.IsDeleted);

            if (existing == null) return null;

            existing.Title       = post.Title;
            existing.Description = post.Description;
            existing.PostType    = post.PostType;
            existing.IsPublished = post.IsPublished;
            existing.UpdatedAt   = DateTime.UtcNow;

            // Replace targets: remove old, add new
            _context.PostTargets.RemoveRange(existing.Targets);
            existing.Targets = post.Targets;

            await _context.SaveChangesAsync();
            return existing;
        }

        // ── TOGGLE PUBLISH ───────────────────────────────────────────
        public async Task<bool> TogglePublish(int postId, int orgId, bool publish)
        {
            var existing = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId && p.OrgId == orgId && !p.IsDeleted);

            if (existing == null) return false;

            existing.IsPublished = publish;
            existing.UpdatedAt   = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── SOFT DELETE ──────────────────────────────────────────────
        public async Task<bool> DeletePost(int postId, int orgId)
        {
            var existing = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId && p.OrgId == orgId && !p.IsDeleted);

            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── COUNT LINKED TASKS ───────────────────────────────────────
        public async Task<int> GetLinkedTaskCount(int postId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .CountAsync(t => t.PostId == postId && !t.IsDeleted);
        }
    }
}
