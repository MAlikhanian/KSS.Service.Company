using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class IndustryController : BaseController<Industry, IndustryDto, IndustryDto, IndustryDto>
    {
        public IndustryController(IIndustryService service) : base(service) { }
    }
}
