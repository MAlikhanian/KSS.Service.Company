using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("CompanyType")]
    public class CompanyTypeController : BaseController<CompanyType, CompanyTypeDto, CompanyTypeDto, CompanyTypeDto>
    {
        public CompanyTypeController(ICompanyTypeService service) : base(service) { }
    }
}
