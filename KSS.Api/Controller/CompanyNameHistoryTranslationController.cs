using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyNameHistoryTranslationController : BaseController<CompanyNameHistoryTranslation, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto>
    {
        public CompanyNameHistoryTranslationController(ICompanyNameHistoryTranslationService service) : base(service) { }
    }
}
