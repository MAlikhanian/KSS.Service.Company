using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class WebsiteLabelService : BaseService<WebsiteLabel, WebsiteLabelDto, WebsiteLabelDto, WebsiteLabelDto>, IWebsiteLabelService
    {
        public WebsiteLabelService(IMapper mapper, IWebsiteLabelRepository repository) : base(mapper, repository) { }
    }
}
