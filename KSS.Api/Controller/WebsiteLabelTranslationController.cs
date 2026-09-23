using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class WebsiteLabelTranslationController : BaseController<WebsiteLabelTranslation, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto, WebsiteLabelTranslationDto>
    {
        public WebsiteLabelTranslationController(IWebsiteLabelTranslationService service) : base(service) { }
    }
}
