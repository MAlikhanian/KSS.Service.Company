using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class AddressController : BaseController<Address, AddressDto, AddressDto, AddressDto>
    {
        public AddressController(IAddressService service) : base(service) { }
    }
}
