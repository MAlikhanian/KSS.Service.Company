using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Industry")]
    public class IndustryController : BaseController<Industry, IndustryDto, IndustryDto, IndustryDto>
    {
        public IndustryController(IIndustryService service) : base(service) { }
    }
}
