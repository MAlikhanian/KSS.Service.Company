using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class CompanyDocumentTypeController : BaseController<CompanyDocumentType, CompanyDocumentTypeDto, CompanyDocumentTypeDto, CompanyDocumentTypeDto>
    {
        public CompanyDocumentTypeController(ICompanyDocumentTypeService service) : base(service) { }
    }
}
