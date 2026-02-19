using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyNameHistoryTranslationService : BaseService<CompanyNameHistoryTranslation, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto>, ICompanyNameHistoryTranslationService
    {
        public CompanyNameHistoryTranslationService(IMapper mapper, ICompanyNameHistoryTranslationRepository repository) : base(mapper, repository) { }
    }
}
