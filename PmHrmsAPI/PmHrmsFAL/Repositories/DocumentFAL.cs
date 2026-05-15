using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsFAL.IRepositories;

namespace PmHrmsAPI.PmHrmsFAL.Repositories
{
    public class DocumentFAL : IDocumentFAL
    {
        private readonly PmHrmsContext _context;

        public DocumentFAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<string> UploadDocumentAsync(IFormFile file, string settingKey)                
        {
            
        
            if (file == null || file.Length == 0)
                throw new Exception("File is required");

            if (file.Length > 10 * 1024 * 1024)
                throw new Exception("Max 10MB allowed");


            var settings = await _context.Settings
                .Where(s => s.Name == "BaseResourcePath" || s.Name == settingKey)
                .ToListAsync();

            // ✅ SAFE lookups
            var baseSetting = settings.FirstOrDefault(s => s.Name == "BaseResourcePath");
            var subSetting = settings.FirstOrDefault(s => s.Name == settingKey);

            if (baseSetting == null)
                throw new Exception("Missing setting: BaseResourcePath");

            if (subSetting == null)
                throw new Exception($"Missing setting: {settingKey}");


            string fullPath = Path.Combine(baseSetting.Value, subSetting.Value.TrimStart('/'));

            if (!Path.IsPathFullyQualified(fullPath))
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), fullPath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);


            string extension = Path.GetExtension(file.FileName).ToLower();

            var allowed = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

            if (!allowed.Contains(extension))
                throw new Exception("Only PDF, Word documents, and Images (.jpg, .png) are allowed");



            string fileName = Guid.NewGuid() + extension;
            string physicalPath = Path.Combine(fullPath, fileName);

            using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.CopyToAsync(stream);

            string dbPath = Path.Combine(subSetting.Value, fileName)
                                .Replace("\\", "/");

            return dbPath;
        }

        public void DeleteDocument(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            string webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(webRoot, fileUrl.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
