using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;
using KSS.Helper.CustomAttribute;

namespace KSS.Api.Controller
{
    [PermissionGroup("Information")]
    public class StakeholderController : BaseController<Stakeholder, StakeholderDto, StakeholderDto, StakeholderDto>
    {
        public StakeholderController(IStakeholderService service) : base(service) { }
    }
}
