using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class WebsiteLabelService : BaseService<WebsiteLabel, WebsiteLabelDto, WebsiteLabelDto, WebsiteLabelDto>, IWebsiteLabelService, IReferenceData
    {
        public WebsiteLabelService(IMapper mapper, IWebsiteLabelRepository repository) : base(mapper, repository) { }
    }
}
