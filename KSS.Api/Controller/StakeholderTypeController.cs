using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("StakeholderType")]
    public class StakeholderTypeController : BaseController<StakeholderType, StakeholderTypeDto, StakeholderTypeDto, StakeholderTypeDto>
    {
        public StakeholderTypeController(IStakeholderTypeService service) : base(service) { }
    }
}
