using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("AddressLabel")]
    public class AddressLabelController : BaseController<AddressLabel, AddressLabelDto, AddressLabelDto, AddressLabelDto>
    {
        public AddressLabelController(IAddressLabelService service) : base(service) { }
    }
}
