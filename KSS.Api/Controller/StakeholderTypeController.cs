using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;
using KSS.Api.Controller;

namespace KSS.Api.Controller
{
    public class StakeholderTypeController : BaseController<StakeholderType, StakeholderTypeDto, StakeholderTypeDto, StakeholderTypeDto>
    {
        public StakeholderTypeController(IStakeholderTypeService service) : base(service) { }
    }
}
