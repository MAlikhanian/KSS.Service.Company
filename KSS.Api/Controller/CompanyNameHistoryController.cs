using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyNameHistoryController : BaseController<CompanyNameHistory, CompanyNameHistoryDto, CompanyNameHistoryDto, CompanyNameHistoryDto>
    {
        public CompanyNameHistoryController(ICompanyNameHistoryService service) : base(service) { }
    }
}
