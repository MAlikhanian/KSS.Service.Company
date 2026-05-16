using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class FinancialInfoController : BaseController<FinancialInfo, FinancialInfoDto, FinancialInfoDto, FinancialInfoDto>
    {
        public FinancialInfoController(IFinancialInfoService service) : base(service) { }
    }
}
