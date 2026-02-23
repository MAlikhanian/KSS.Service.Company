using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyDetailService
    {
        Task<CompanyDetailDto?> GetByIdAsync(Guid id, short languageId = 12);
        Task<CompanyDetailDto> UpdateAsync(Guid id, CompanyDetailDto dto);
    }
}
