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
    [HasPermission("Company.Read")]
    public class CompanySelectController : ControllerBase
    {
        private readonly ICompanySelectService _service;

        public CompanySelectController(ICompanySelectService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /Api/CompanySelect/List?languageId=12&query=کارگزاری
        /// Returns a lightweight list of companies for select dropdowns.
        /// </summary>
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<CompanySelectDto>>> List(
            [FromQuery] short languageId = 12,
            [FromQuery] string? query = null)
        {
            var result = await _service.GetCompanySelectListAsync(languageId, query);
            return Ok(result);
        }
    }
}
