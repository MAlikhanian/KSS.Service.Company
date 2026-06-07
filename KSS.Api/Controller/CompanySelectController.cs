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
    [HasPermission("Company.Information.Read")]
    public class CompanySelectController : ControllerBase
    {
        private readonly ICompanySelectService _service;

        public CompanySelectController(ICompanySelectService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /Api/CompanySelect/List?languageId=12&query=کارگزاری&companyIds=g1&companyIds=g2
        /// Returns a lightweight list of companies for select dropdowns.
        /// companyIds, when supplied, narrows the result to the intersection with
        /// the caller's Access/RoleAccess set — never widens it.
        /// </summary>
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<CompanySelectDto>>> List(
            [FromQuery] short languageId = 12,
            [FromQuery] string? query = null,
            [FromQuery] Guid[]? companyIds = null)
        {
            var result = await _service.GetCompanySelectListAsync(languageId, query, companyIds);
            return Ok(result);
        }
    }
}
