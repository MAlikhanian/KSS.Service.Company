using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class EmailLabelController : BaseController<EmailLabel, EmailLabelDto, EmailLabelDto, EmailLabelDto>
    {
        public EmailLabelController(IEmailLabelService service) : base(service) { }
    }
}
