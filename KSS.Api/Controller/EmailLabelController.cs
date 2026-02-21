using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("EmailLabel")]
    public class EmailLabelController : BaseController<EmailLabel, EmailLabelDto, EmailLabelDto, EmailLabelDto>
    {
        public EmailLabelController(IEmailLabelService service) : base(service) { }
    }
}
