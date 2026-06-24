using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class EmailController : BaseController<Email, EmailDto, EmailInsertDto, EmailDto>
    {
        public EmailController(IEmailService service) : base(service) { }
    }
}
