using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyTranslationService : BaseService<CompanyTranslation, CompanyTranslationDto, CompanyTranslationDto, CompanyTranslationDto>, ICompanyTranslationService
    {
        public CompanyTranslationService(IMapper mapper, ICompanyTranslationRepository repository) : base(mapper, repository) { }
    }
}
