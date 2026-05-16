using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class TranslationController : BaseController<Translation, TranslationDto, TranslationDto, TranslationDto>
    {
        public TranslationController(ITranslationService service) : base(service) { }
    }
}
