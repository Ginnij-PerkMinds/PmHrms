using Microsoft.AspNetCore.Http;

namespace PmHrmsAPI.PmHrmsFAL.IRespositories
{
    public interface IImageFAL
    {
       
        Task<string> UploadImageAsync(IFormFile file, string settingKey);

        
        void DeleteImage(string fileUrl);
    }
}