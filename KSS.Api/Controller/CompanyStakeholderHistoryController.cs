using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyStakeholderHistoryController : BaseController<CompanyStakeholderHistory, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto>
    {
        public CompanyStakeholderHistoryController(ICompanyStakeholderHistoryService service) : base(service) { }
    }
}
