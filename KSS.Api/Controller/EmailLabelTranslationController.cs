using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("EmailLabel")]
    public class EmailLabelTranslationController : BaseController<EmailLabelTranslation, EmailLabelTranslationDto, EmailLabelTranslationDto, EmailLabelTranslationDto>
    {
        public EmailLabelTranslationController(IEmailLabelTranslationService service) : base(service) { }
    }
}
