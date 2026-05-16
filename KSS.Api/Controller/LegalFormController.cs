using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class LegalFormController : BaseController<LegalForm, LegalFormDto, LegalFormDto, LegalFormDto>
    {
        public LegalFormController(ILegalFormService service) : base(service) { }
    }
}
