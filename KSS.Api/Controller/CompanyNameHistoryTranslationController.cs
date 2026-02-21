using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Company")]
    public class CompanyNameHistoryTranslationController : BaseController<CompanyNameHistoryTranslation, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto>
    {
        public CompanyNameHistoryTranslationController(ICompanyNameHistoryTranslationService service) : base(service) { }
    }
}
