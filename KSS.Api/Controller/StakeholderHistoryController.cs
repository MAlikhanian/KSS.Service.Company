using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class StakeholderHistoryController : BaseController<StakeholderHistory, StakeholderHistoryDto, StakeholderHistoryDto, StakeholderHistoryDto>
    {
        public StakeholderHistoryController(IStakeholderHistoryService service) : base(service) { }
    }
}
