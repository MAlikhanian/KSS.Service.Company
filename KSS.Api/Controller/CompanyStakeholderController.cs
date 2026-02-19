using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyStakeholderController : BaseController<CompanyStakeholder, CompanyStakeholderDto, CompanyStakeholderDto, CompanyStakeholderDto>
    {
        public CompanyStakeholderController(ICompanyStakeholderService service) : base(service) { }
    }
}
