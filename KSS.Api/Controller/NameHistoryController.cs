using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class NameHistoryController : BaseController<NameHistory, NameHistoryDto, NameHistoryDto, NameHistoryDto>
    {
        public NameHistoryController(INameHistoryService service) : base(service) { }
    }
}
