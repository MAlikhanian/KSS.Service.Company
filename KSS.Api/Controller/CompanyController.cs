using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyController : BaseController<Company, CompanyDto, CompanyDto, CompanyDto>
    {
        public CompanyController(ICompanyService service) : base(service) { }
    }
}
