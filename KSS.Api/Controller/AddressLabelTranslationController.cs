using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("AddressLabel")]
    public class AddressLabelTranslationController : BaseController<AddressLabelTranslation, AddressLabelTranslationDto, AddressLabelTranslationDto, AddressLabelTranslationDto>
    {
        public AddressLabelTranslationController(IAddressLabelTranslationService service) : base(service) { }
    }
}
