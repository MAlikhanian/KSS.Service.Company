using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;
using Microsoft.AspNetCore.Mvc;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class FinancialInfoController : BaseController<FinancialInfo, FinancialInfoDto, FinancialInfoInsertDto, FinancialInfoDto>
    {
        private readonly IFinancialInfoService _financialInfoService;

        public FinancialInfoController(IFinancialInfoService service) : base(service)
        {
            _financialInfoService = service;
        }

        /// <summary>
        /// GET /Api/FinancialInfo/ByCompany/{companyId} — a company's financial-info
        /// rows. Report read (no per-caller access filter); used by the Members
        /// brokerage-profile report for registered capital.
        /// </summary>
        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult> ByCompany(Guid companyId)
            => Ok(await _financialInfoService.GetByCompanyAsync(companyId));
    }
}
