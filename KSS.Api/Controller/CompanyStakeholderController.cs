using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Stakeholder")]
    public class CompanyStakeholderController : BaseController<CompanyStakeholder, CompanyStakeholderDto, CompanyStakeholderDto, CompanyStakeholderDto>
    {
        public CompanyStakeholderController(ICompanyStakeholderService service) : base(service) { }
    }
}
