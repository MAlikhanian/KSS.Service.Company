using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class PhoneController : BaseController<Phone, PhoneDto, PhoneDto, PhoneDto>
    {
        public PhoneController(IPhoneService service) : base(service) { }
    }
}
