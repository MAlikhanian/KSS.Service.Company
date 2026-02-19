using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyOperationService
    {
        Task<CompanyDto> CreateCompanyWithTranslationsAndNameHistoryAsync(CompanyInsertDto dto);
    }
}
