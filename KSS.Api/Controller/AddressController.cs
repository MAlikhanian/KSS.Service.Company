using KSS.Dto;
using KSS.Entity;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    [PermissionGroup("Address")]
    public class AddressController : BaseController<Address, AddressDto, AddressDto, AddressDto>
    {
        public AddressController(IAddressService service) : base(service) { }
    }
}
