using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Address")]
    public class AddressTranslationController : BaseController<AddressTranslation, AddressTranslationDto, AddressTranslationDto, AddressTranslationDto>
    {
        public AddressTranslationController(IAddressTranslationService service) : base(service) { }
    }
}
