using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class PhoneLabelTranslationController : BaseController<PhoneLabelTranslation, PhoneLabelTranslationDto, PhoneLabelTranslationDto, PhoneLabelTranslationDto>
    {
        public PhoneLabelTranslationController(IPhoneLabelTranslationService service) : base(service) { }
    }
}
