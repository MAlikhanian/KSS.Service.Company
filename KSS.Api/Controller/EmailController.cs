using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class EmailController : BaseController<Email, EmailDto, EmailDto, EmailDto>
    {
        public EmailController(IEmailService service) : base(service) { }
    }
}
