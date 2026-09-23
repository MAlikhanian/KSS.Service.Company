using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class WebsiteController : BaseController<Website, WebsiteDto, WebsiteInsertDto, WebsiteDto>
    {
        public WebsiteController(IWebsiteService service) : base(service) { }
    }
}
