using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsFAL.IRespositories;

namespace PmHrmsAPI.PmHrmsFAL.Repositories
{
    public class ImageFAL : IImageFAL
    {
        private readonly PmHrmsContext _context;

        public ImageFAL(PmHrmsContext context)
        {
            _context = context;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string settingKey)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new Exception("Image file is required");

                // Max 2 MB
                if (file.Length > 2 * 1024 * 1024)
                    throw new Exception("Image size must be less than 2 MB");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    throw new Exception("Only JPG and PNG images are allowed");

                var settings = await _context.Settings
                    .Where(s => s.Name == "BaseResourcePath" || s.Name == settingKey)
                    .ToListAsync();

                var basePathSetting = settings.FirstOrDefault(s => s.Name == "BaseResourcePath");
                var subDirSetting = settings.FirstOrDefault(s => s.Name == settingKey);

                if (basePathSetting == null || subDirSetting == null)
                    throw new Exception("Image path settings are missing");

                string fullPathString = basePathSetting.Value + subDirSetting.Value;

                if (!Path.IsPathFullyQualified(fullPathString))
                    fullPathString = Path.Combine(Directory.GetCurrentDirectory(), fullPathString);

                if (!Directory.Exists(fullPathString))
                    Directory.CreateDirectory(fullPathString);

                string uniqueFileName = Guid.NewGuid() + extension;
                string physicalPath = Path.Combine(fullPathString, uniqueFileName);

                using var fileStream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                string dbPath = (basePathSetting.Value + subDirSetting.Value)
                    .Replace("wwwroot", "")
                    + "/" + uniqueFileName;

                return dbPath.Replace("\\", "/");
            }
            catch (Exception ex)
            {
                throw new Exception("Image upload failed: " + ex.Message);
            }
        }


        public void DeleteImage(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                
                string webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

              
                string fullPath = Path.Combine(webRoot, fileUrl.TrimStart('/').Replace("/", "\\"));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                
            }
        }
    }
}