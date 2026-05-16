using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class NameHistoryTranslationController : BaseController<NameHistoryTranslation, NameHistoryTranslationDto, NameHistoryTranslationDto, NameHistoryTranslationDto>
    {
        public NameHistoryTranslationController(INameHistoryTranslationService service) : base(service) { }
    }
}
