using System.Security.Claims;
using KSS.Dto;
using KSS.Helper;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    [Authorize]
    // [HasPermission("Company.Access.Read")] is now per-action so
    // ListAllGrants can be accessible to any authenticated user.
    public class AccessController : ControllerBase
    {
        private readonly IAccessService _service;

        public AccessController(IAccessService service)
        {
            _service = service;
        }

        /// <summary>GET /Api/Access/ByCompany/{companyId} — list grants on a company, one entry per grantee.</summary>
        [HttpGet("{companyId}")]
        [HasPermission("Company.Access.Read")]
        public async Task<ActionResult<List<AccessGrantSummaryDto>>> ByCompany(Guid companyId)
        {
            var caller = GetCallerPersonId();
            // Non-owners need at least View on the access section (level >= 1)
            // to see the company's grant list.
            var levels = await _service.GetLevelsAsync(companyId, caller);
            if (levels.Access < 1)
                throw new BusinessRuleException("شما اجازه مشاهده دسترسی‌های این شرکت را ندارید");

            var data = await _service.ListGrantsByCompanyAsync(companyId);
            return Ok(data);
        }

        /// <summary>POST /Api/Access/Grant — upsert a grant (2 section levels) for a (companyId, grantedToPersonId) pair.</summary>
        [HttpPost]
        [HasPermission("Company.Access.Modify")]
        public async Task<ActionResult> Grant([FromBody] AccessGrantDto dto)
        {
            var caller = GetCallerPersonId();
            await _service.UpsertGrantAsync(dto, caller);
            return NoContent();
        }

        /// <summary>POST /Api/Access/RevokeByPair/{companyId}/{grantedToPersonId} — revoke all rows for the pair.</summary>
        [HttpPost("{companyId}/{grantedToPersonId}")]
        [HasPermission("Company.Access.Modify")]
        public async Task<ActionResult> RevokeByPair(Guid companyId, Guid grantedToPersonId)
        {
            var caller = GetCallerPersonId();
            await _service.RevokeByPairAsync(companyId, grantedToPersonId, caller);
            return NoContent();
        }

        /// <summary>GET /Api/Access/MyLevels/{companyId} — caller's per-section levels on companyId.</summary>
        [HttpGet("{companyId}")]
        [HasPermission("Company.Access.Read")]
        public async Task<ActionResult<AccessLevelsDto>> MyLevels(Guid companyId)
        {
            var levels = await _service.GetLevelsAsync(companyId, GetCallerPersonId());
            return Ok(levels);
        }

        /// <summary>
        /// GET /Api/Access/ListAllGrants — flat list of (CompanyId, GrantedToPersonId)
        /// pairs across all companies. Powers the dashboard Highlights panel.
        /// Available to any authenticated user (no specific permission required).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<AccessGrantPairDto>>> ListAllGrants()
        {
            var data = await _service.ListAllGrantPairsAsync();
            return Ok(data);
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
