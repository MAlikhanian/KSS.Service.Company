using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class AddressTranslationController : BaseController<AddressTranslation, AddressTranslationDto, AddressTranslationDto, AddressTranslationDto>
    {
        public AddressTranslationController(IAddressTranslationService service) : base(service) { }
    }
}
