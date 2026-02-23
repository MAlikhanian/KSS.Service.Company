using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanySelectService
    {
        Task<IEnumerable<CompanySelectDto>> GetCompanySelectListAsync(short languageId, string? query = null);
    }
}
