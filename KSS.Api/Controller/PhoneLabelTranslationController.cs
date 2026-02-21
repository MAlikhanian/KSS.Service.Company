using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("PhoneLabel")]
    public class PhoneLabelTranslationController : BaseController<PhoneLabelTranslation, PhoneLabelTranslationDto, PhoneLabelTranslationDto, PhoneLabelTranslationDto>
    {
        public PhoneLabelTranslationController(IPhoneLabelTranslationService service) : base(service) { }
    }
}
