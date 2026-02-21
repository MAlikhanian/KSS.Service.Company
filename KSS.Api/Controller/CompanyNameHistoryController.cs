using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Company")]
    public class CompanyNameHistoryController : BaseController<CompanyNameHistory, CompanyNameHistoryDto, CompanyNameHistoryDto, CompanyNameHistoryDto>
    {
        public CompanyNameHistoryController(ICompanyNameHistoryService service) : base(service) { }
    }
}
