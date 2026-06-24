using KSS.Dto;
using KSS.Entity;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using KSS.Api.Controller;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class AddressController : BaseController<Address, AddressDto, AddressInsertDto, AddressDto>
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService service) : base(service)
        {
            _addressService = service;
        }

        /// <summary>
        /// GET /Api/Address/ByCompany/{companyId} — a company's address rows.
        /// Report read (no per-caller access filter); used by the Members
        /// brokerage-profile report for the registration location.
        /// </summary>
        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult> ByCompany(Guid companyId)
            => Ok(await _addressService.GetByCompanyAsync(companyId));
    }
}
