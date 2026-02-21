using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Email")]
    public class EmailController : BaseController<Email, EmailDto, EmailDto, EmailDto>
    {
        public EmailController(IEmailService service) : base(service) { }
    }
}
