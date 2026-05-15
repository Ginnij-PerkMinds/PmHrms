using PmHrmsAPI.PmHrmsBAL.ResponseModel;

namespace PmHrmsAPI.PmHrmsBAL.IRepsitories
{
    public interface IGlobalSearchBAL
    {
        Task<GlobalSearchResponseModel> Search(string searchTerm, int orgId, int limit, string scope);
    }
}
