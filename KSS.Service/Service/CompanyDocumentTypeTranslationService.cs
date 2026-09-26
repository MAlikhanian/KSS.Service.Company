using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class CompanyDocumentTypeTranslationService : BaseService<CompanyDocumentTypeTranslation, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto>, ICompanyDocumentTypeTranslationService, IReferenceData
    {
        public CompanyDocumentTypeTranslationService(IMapper mapper, ICompanyDocumentTypeTranslationRepository repository) : base(mapper, repository) { }
    }
}
