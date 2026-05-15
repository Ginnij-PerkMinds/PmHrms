using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

namespace PmHrmsAPI.PmHrmsBAL.Services.Interfaces
{
    public interface IMigrationService
    {

        Task<List<SystemField>> GetConfigsByEntityAsync(string entityType);
        Dictionary<string, object> AutoMapWithConfidence(
             List<string> excelColumns,
             List<SystemField> systemFields,
             List<Dictionary<string, object>> sampleData);

        Task<MigrationJob?> GetActiveJobAsync(int orgId, string entityType);

        Task<object> ValidateFullFileAsync(ImportRequestModel request, int orgId, PmHrmsContext? db = null);
        byte[] GenerateExcelTemplate(List<string> headers);

        Task<Guid> CreateJobAsync(int orgId, int userId, string entityType, int totalRecords, string? fileName);

        Task ProcessImportAsync(Guid jobId, ImportRequestModel request, int orgId);
        
        Task<MigrationJob?> GetJobByIdAsync(Guid jobId);
        Task<(bool Success, string Message)> CancelJobAsync(Guid jobId);
        Task<List<MigrationJob>> GetHistoryAsync(int orgId, string entityType);

    }
}
