using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyDocumentTypeTranslationService : BaseService<CompanyDocumentTypeTranslation, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto>, ICompanyDocumentTypeTranslationService
    {
        public CompanyDocumentTypeTranslationService(IMapper mapper, ICompanyDocumentTypeTranslationRepository repository) : base(mapper, repository) { }
    }
}
