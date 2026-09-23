using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class CompanyDocumentTypeTranslationController : BaseController<CompanyDocumentTypeTranslation, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto, CompanyDocumentTypeTranslationDto>
    {
        public CompanyDocumentTypeTranslationController(ICompanyDocumentTypeTranslationService service) : base(service) { }
    }
}
