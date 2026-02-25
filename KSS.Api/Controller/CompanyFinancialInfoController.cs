using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Company")]
    public class CompanyFinancialInfoController : BaseController<CompanyFinancialInfo, CompanyFinancialInfoDto, CompanyFinancialInfoDto, CompanyFinancialInfoDto>
    {
        public CompanyFinancialInfoController(ICompanyFinancialInfoService service) : base(service) { }
    }
}
