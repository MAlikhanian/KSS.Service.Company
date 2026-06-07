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
    public class CompanyStakeholderManagementController : ControllerBase
    {
        private readonly ICompanyStakeholderManagementService _service;

        public CompanyStakeholderManagementController(ICompanyStakeholderManagementService service)
        {
            _service = service;
        }

        [HttpGet("ByCompany/{companyId}")]
        [HasPermission("Company.Information.Read")]
        public async Task<ActionResult<List<CompanyStakeholderViewDto>>> GetByCompany(Guid companyId, [FromQuery] short languageId = 12)
        {
            var result = await _service.GetByCompanyAsync(companyId, GetCallerPersonId(), languageId);
            if (result == null)
                return NotFound(new { message = "Company not found." });
            return Ok(result);
        }

        [HttpPost("{companyId}")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult<CompanyStakeholderViewDto>> Add(Guid companyId, [FromBody] CompanyStakeholderUpsertDto dto, [FromQuery] short languageId = 12)
        {
            var result = await _service.AddAsync(companyId, GetCallerPersonId(), dto, languageId);
            return Ok(result);
        }

        [HttpPut("{stakeholderId}")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult<CompanyStakeholderViewDto>> Update(Guid stakeholderId, [FromBody] CompanyStakeholderUpsertDto dto, [FromQuery] short languageId = 12)
        {
            var result = await _service.UpdateAsync(stakeholderId, GetCallerPersonId(), dto, languageId);
            return Ok(result);
        }

        [HttpDelete("{stakeholderId}")]
        [HasPermission("Company.Information.Modify")]
        public async Task<ActionResult> Delete(Guid stakeholderId)
        {
            await _service.DeleteAsync(stakeholderId, GetCallerPersonId());
            return NoContent();
        }

        private Guid GetCallerPersonId()
        {
            var raw = User.FindFirstValue("personId")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var personId))
                throw new BusinessRuleException("Caller PersonId not found on the JWT.");
            return personId;
        }
    }
}
