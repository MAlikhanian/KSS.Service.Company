using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class SoftwareCategoryController : BaseController<SoftwareCategory, SoftwareCategoryDto, SoftwareCategoryDto, SoftwareCategoryDto>
    {
        public SoftwareCategoryController(ISoftwareCategoryService service) : base(service) { }
    }
}
