using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class WebsiteLabelTranslationService : BaseService<WebsiteLabelTranslation, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto>, IWebsiteLabelTranslationService
    {
        public WebsiteLabelTranslationService(IMapper mapper, IWebsiteLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
