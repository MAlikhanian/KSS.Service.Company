using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KSS.Dto;
using KSS.Helper;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
    public class CompanySoftwareController : ControllerBase
    {
        private readonly ICompanySoftwareManagementService _service;
        public CompanySoftwareController(ICompanySoftwareManagementService service) => _service = service;

        [HttpGet("{companyId:guid}")]
        [HasPermission("Company.Information.Read")]
        public async Task<ActionResult<List<CompanySoftwareSlotDto>>> GetSlots(Guid companyId, [FromQuery] short languageId = 12)
        {
            var result = await _service.GetSlotsAsync(companyId, GetCallerPersonId(), languageId);
            if (result == null) return NotFound(new { message = "Company not found." });
            return Ok(result);
        }

        [HttpGet("catalog")]
        [HasPermission("Company.Information.Read")]
        public async Task<ActionResult<List<SoftwareCatalogItemDto>>> GetCatalog([FromQuery] short languageId = 12)
        {
            return Ok(await _service.GetSoftwareCatalogAsync(languageId));
        }

        [HttpPut("{companyId:guid}")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult> Upsert(Guid companyId, [FromBody] CompanySoftwareUpsertDto dto)
        {
            await _service.UpsertAsync(companyId, dto);
            return NoContent();
        }

        [HttpDelete("{companyId:guid}/{softwareCategoryId}")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult> Clear(Guid companyId, byte softwareCategoryId)
        {
            await _service.ClearAsync(companyId, softwareCategoryId);
            return NoContent();
        }

        private Guid GetCallerPersonId()
        {
            var raw = User.FindFirstValue("personId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var personId))
                throw new BusinessRuleException("Caller PersonId not found on the JWT.");
            return personId;
        }
    }
}
