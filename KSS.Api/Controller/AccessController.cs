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

        /// <summary>
        /// GET /Api/Access/MyGrants — the signed-in caller's own grants: her personal grants and
        /// the grants of her roles, live or not, with their flags, and the companies they name.
        /// A grant that covers every company names none and is not expanded. The caller and her
        /// roles are read from the token only; nothing in the request can name another person.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<MyGrantsDto>> MyGrants([FromServices] IMyGrantsService myGrants)
        {
            var result = await myGrants.GetAsync(GetCallerPersonId(), GetCallerRoleIds());
            return Ok(result);
        }

        // The caller's roles, from the token's roleId claims: the same source the access levels use.
        private List<Guid> GetCallerRoleIds() => User.FindAll("roleId")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

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
