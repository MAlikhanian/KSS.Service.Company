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
    [HasPermission("Company.Access.Read")]
    public class RoleAccessController : ControllerBase
    {
        private readonly IRoleAccessService _service;
        private readonly IAccessService _accessService;

        public RoleAccessController(IRoleAccessService service, IAccessService accessService)
        {
            _service = service;
            _accessService = accessService;
        }

        /// <summary>GET /Api/RoleAccess/ByCompany/{companyId} — per-company + global role grants for this company.</summary>
        [HttpGet("{companyId}")]
        public async Task<ActionResult<List<RoleAccessGrantSummaryDto>>> ByCompany(Guid companyId)
        {
            var caller = GetCallerPersonId();
            var levels = await _accessService.GetLevelsAsync(companyId, caller);
            if (levels.Access < 1)
                throw new BusinessRuleException("شما اجازه مشاهده دسترسی‌های این شرکت را ندارید");

            var data = await _service.ListGrantsByCompanyAsync(companyId);
            return Ok(data);
        }

        /// <summary>GET /Api/RoleAccess/All — every role grant in the system. Manage-only.</summary>
        [HttpGet]
        [HasPermission("Company.Access.Modify")]
        public async Task<ActionResult<List<RoleAccessGrantSummaryDto>>> All()
        {
            var data = await _service.ListAllGrantsAsync();
            return Ok(data);
        }

        /// <summary>POST /Api/RoleAccess/Grant — upsert a role grant for (companyId|null, grantedToRoleId).</summary>
        [HttpPost]
        [HasPermission("Company.Access.Modify")]
        public async Task<ActionResult> Grant([FromBody] RoleAccessGrantDto dto)
        {
            var caller = GetCallerPersonId();
            await _service.UpsertGrantAsync(dto, caller);
            return NoContent();
        }

        /// <summary>POST /Api/RoleAccess/RevokeByPair — revoke a (companyId|null, grantedToRoleId) pair.</summary>
        [HttpPost]
        [HasPermission("Company.Access.Modify")]
        public async Task<ActionResult> RevokeByPair([FromQuery] Guid? companyId, [FromQuery] Guid grantedToRoleId)
        {
            var caller = GetCallerPersonId();
            await _service.RevokeByPairAsync(companyId, grantedToRoleId, caller);
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
