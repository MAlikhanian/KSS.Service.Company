using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KSS.Dto;
using KSS.Helper;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    /// <summary>
    /// Read-only consolidated company view for display screens that need
    /// company core + name history + contacts in one request.
    ///
    /// Country/Region/City IDs are returned as-is; the BFF resolves them
    /// to names against KSS.Service.Common before responding to the frontend.
    /// </summary>
    [ApiController]
    [Route("Api/[controller]")]
    [Authorize]
    [HasPermission("Company.Information.Read")]
    public class CompanyReadViewController : ControllerBase
    {
        private readonly ICompanyReadViewManagementService _service;

        public CompanyReadViewController(ICompanyReadViewManagementService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyReadViewDto>> GetById(Guid id, [FromQuery] short languageId = 12)
        {
            var result = await _service.GetByIdAsync(id, GetCallerPersonId(), languageId);
            if (result == null)
                return NotFound(new { message = "Company not found." });
            return Ok(result);
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
