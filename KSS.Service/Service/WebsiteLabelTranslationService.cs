using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class WebsiteLabelTranslationService : BaseService<WebsiteLabelTranslation, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto>, IWebsiteLabelTranslationService, IReferenceData
    {
        public WebsiteLabelTranslationService(IMapper mapper, IWebsiteLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
