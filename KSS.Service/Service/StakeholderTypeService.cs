using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class StakeholderTypeService : BaseService<StakeholderType, StakeholderTypeDto, StakeholderTypeDto, StakeholderTypeDto>, IStakeholderTypeService
    {
        public StakeholderTypeService(IMapper mapper, IStakeholderTypeRepository repository) : base(mapper, repository) { }
    }
}
