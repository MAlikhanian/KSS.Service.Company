using System.Security.Claims;
using KSS.Dto;
using KSS.Helper;
using KSS.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    [Authorize]
    public class CompanyOwnershipController : ControllerBase
    {
        private readonly ICompanyOwnershipService _service;

        public CompanyOwnershipController(ICompanyOwnershipService service)
        {
            _service = service;
        }

        /// <summary>POST /Api/CompanyOwnership/Assign — assign a company record to an owner company.</summary>
        [HttpPost]
        public async Task<ActionResult<CompanyOwnershipViewDto>> Assign([FromBody] CompanyOwnershipInsertDto dto)
        {
            var result = await _service.AssignAsync(dto, GetCallerPersonId());
            return Ok(result);
        }

        /// <summary>POST /Api/CompanyOwnership/Unassign/{ownerCompanyId}/{companyId} — remove the ownership link.</summary>
        [HttpPost("{ownerCompanyId}/{companyId}")]
        public async Task<ActionResult> Unassign(Guid ownerCompanyId, Guid companyId)
        {
            await _service.UnassignAsync(ownerCompanyId, companyId);
            return NoContent();
        }

        /// <summary>GET /Api/CompanyOwnership/CompaniesByOwner/{ownerCompanyId} — company ids owned by an owner company.</summary>
        [HttpGet("{ownerCompanyId}")]
        public async Task<ActionResult<List<Guid>>> CompaniesByOwner(Guid ownerCompanyId)
        {
            return Ok(await _service.ListCompanyIdsByOwnerAsync(ownerCompanyId));
        }

        /// <summary>GET /Api/CompanyOwnership/OwnersByCompany/{companyId} — owner-company ids that own a company.</summary>
        [HttpGet("{companyId}")]
        public async Task<ActionResult<List<Guid>>> OwnersByCompany(Guid companyId)
        {
            return Ok(await _service.ListOwnerIdsByCompanyAsync(companyId));
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
