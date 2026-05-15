using Microsoft.AspNetCore.Http;

namespace PmHrmsAPI.PmHrmsFAL.IRepositories
{
    public interface IDocumentFAL
    {
        Task<string> UploadDocumentAsync(IFormFile file, string settingKey);
        void DeleteDocument(string fileUrl);
    }
}
