using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class WebsiteLabelController : BaseController<WebsiteLabel, WebsiteLabelDto, WebsiteLabelDto, WebsiteLabelDto>
    {
        public WebsiteLabelController(IWebsiteLabelService service) : base(service) { }
    }
}
