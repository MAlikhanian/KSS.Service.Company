using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class CompanyTypeController : BaseController<CompanyType, CompanyTypeDto, CompanyTypeDto, CompanyTypeDto>
    {
        public CompanyTypeController(ICompanyTypeService service) : base(service) { }
    }
}
