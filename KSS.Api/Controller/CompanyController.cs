using KSS.Dto;
using KSS.Entity;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using KSS.Api.Controller;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class CompanyController : BaseController<Company, CompanyDto, CompanyDto, CompanyDto>
    {
        private readonly ICompanyService _service;

        public CompanyController(ICompanyService service) : base(service)
        {
            _service = service;
        }

        /// <summary>GET /Api/Company/Count — scalar total companies in the tenant. Powers the dashboard tile.</summary>
        [HttpGet]
        public async Task<ActionResult<CompanyCountDto>> Count()
        {
            var total = await _service.CountAsync();
            return Ok(new CompanyCountDto { Count = total });
        }
    }
}
