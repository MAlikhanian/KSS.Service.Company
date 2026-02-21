using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("PhoneLabel")]
    public class PhoneLabelController : BaseController<PhoneLabel, PhoneLabelDto, PhoneLabelDto, PhoneLabelDto>
    {
        public PhoneLabelController(IPhoneLabelService service) : base(service) { }
    }
}
