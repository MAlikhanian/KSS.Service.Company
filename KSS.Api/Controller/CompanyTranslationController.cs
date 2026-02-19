using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyTranslationController : BaseController<CompanyTranslation, CompanyTranslationDto, CompanyTranslationDto, CompanyTranslationDto>
    {
        public CompanyTranslationController(ICompanyTranslationService service) : base(service) { }
    }
}
