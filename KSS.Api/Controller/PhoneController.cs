using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class PhoneController : BaseController<Phone, PhoneDto, PhoneDto, PhoneDto>
    {
        public PhoneController(IPhoneService service) : base(service) { }
    }
}
