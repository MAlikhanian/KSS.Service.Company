using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KSS.Dto;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
    public class CompanyContactController : ControllerBase
    {
        private readonly ICompanyContactService _service;

        public CompanyContactController(ICompanyContactService service)
        {
            _service = service;
        }

        /// <summary>GET /Api/CompanyContact/{companyId}?languageId=12</summary>
        [HttpGet("{companyId}")]
        [HasPermission("Company.Read")]
        public async Task<ActionResult<CompanyContactDto>> GetAll(Guid companyId, [FromQuery] short languageId = 12)
        {
            var result = await _service.GetContactDataAsync(companyId, languageId);
            return Ok(result);
        }

        // --- Email CRUD ---
        [HttpPost("{companyId}/Email")]
        [HasPermission("Email.Create")]
        public async Task<ActionResult<CompanyEmailViewDto>> AddEmail(Guid companyId, [FromBody] CompanyEmailViewDto dto)
        {
            var result = await _service.AddEmailAsync(companyId, dto);
            return Ok(result);
        }

        [HttpPut("Email/{emailId}")]
        [HasPermission("Email.Update")]
        public async Task<ActionResult<CompanyEmailViewDto>> UpdateEmail(Guid emailId, [FromBody] CompanyEmailViewDto dto)
        {
            var result = await _service.UpdateEmailAsync(emailId, dto);
            return Ok(result);
        }

        [HttpDelete("Email/{emailId}")]
        [HasPermission("Email.Delete")]
        public async Task<ActionResult> DeleteEmail(Guid emailId)
        {
            await _service.DeleteEmailAsync(emailId);
            return NoContent();
        }

        // --- Phone CRUD ---
        [HttpPost("{companyId}/Phone")]
        [HasPermission("Phone.Create")]
        public async Task<ActionResult<CompanyPhoneViewDto>> AddPhone(Guid companyId, [FromBody] CompanyPhoneViewDto dto)
        {
            var result = await _service.AddPhoneAsync(companyId, dto);
            return Ok(result);
        }

        [HttpPut("Phone/{phoneId}")]
        [HasPermission("Phone.Update")]
        public async Task<ActionResult<CompanyPhoneViewDto>> UpdatePhone(Guid phoneId, [FromBody] CompanyPhoneViewDto dto)
        {
            var result = await _service.UpdatePhoneAsync(phoneId, dto);
            return Ok(result);
        }

        [HttpDelete("Phone/{phoneId}")]
        [HasPermission("Phone.Delete")]
        public async Task<ActionResult> DeletePhone(Guid phoneId)
        {
            await _service.DeletePhoneAsync(phoneId);
            return NoContent();
        }

        // --- Address CRUD ---
        [HttpPost("{companyId}/Address")]
        [HasPermission("Address.Create")]
        public async Task<ActionResult<CompanyAddressViewDto>> AddAddress(Guid companyId, [FromBody] CompanyAddressViewDto dto, [FromQuery] short languageId = 12)
        {
            var result = await _service.AddAddressAsync(companyId, dto, languageId);
            return Ok(result);
        }

        [HttpPut("Address/{addressId}")]
        [HasPermission("Address.Update")]
        public async Task<ActionResult<CompanyAddressViewDto>> UpdateAddress(Guid addressId, [FromBody] CompanyAddressViewDto dto, [FromQuery] short languageId = 12)
        {
            var result = await _service.UpdateAddressAsync(addressId, dto, languageId);
            return Ok(result);
        }

        [HttpDelete("Address/{addressId}")]
        [HasPermission("Address.Delete")]
        public async Task<ActionResult> DeleteAddress(Guid addressId)
        {
            await _service.DeleteAddressAsync(addressId);
            return NoContent();
        }
    }
}
