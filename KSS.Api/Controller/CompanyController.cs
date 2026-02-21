using KSS.Dto;
using KSS.Entity;
using KSS.Helper.CustomAttribute;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    [PermissionGroup("Company")]
    public class CompanyController : BaseController<Company, CompanyDto, CompanyDto, CompanyDto>
    {
        public CompanyController(ICompanyService service) : base(service) { }
    }
}
