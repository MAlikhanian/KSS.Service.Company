using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class EmailLabelTranslationController : BaseController<EmailLabelTranslation, EmailLabelTranslationDto, EmailLabelTranslationDto, EmailLabelTranslationDto>
    {
        public EmailLabelTranslationController(IEmailLabelTranslationService service) : base(service) { }
    }
}
