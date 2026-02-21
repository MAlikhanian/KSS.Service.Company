using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Stakeholder")]
    public class CompanyStakeholderHistoryController : BaseController<CompanyStakeholderHistory, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto>
    {
        public CompanyStakeholderHistoryController(ICompanyStakeholderHistoryService service) : base(service) { }
    }
}
