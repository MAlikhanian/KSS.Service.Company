using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class AddressLabelTranslationController : BaseController<AddressLabelTranslation, AddressLabelTranslationDto, AddressLabelTranslationDto, AddressLabelTranslationDto>
    {
        public AddressLabelTranslationController(IAddressLabelTranslationService service) : base(service) { }
    }
}
