using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class StakeholderService : BaseService<Stakeholder, StakeholderDto, StakeholderDto, StakeholderDto>, IStakeholderService
    {
        public StakeholderService(IMapper mapper, IStakeholderRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(Stakeholder item, bool saveChanges = true)
        {
            ValidateStakeholder(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(StakeholderDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<Stakeholder>(item);
            ValidateStakeholder(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(Stakeholder item, bool saveChanges = true)
        {
            ValidateStakeholder(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(StakeholderDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<Stakeholder>(item);
            ValidateStakeholder(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidateStakeholder(Stakeholder stakeholder)
        {
            // Validate RelatedPartyType: must be 1 (Company) or 2 (Person)
            if (stakeholder.RelatedPartyType != 1 && stakeholder.RelatedPartyType != 2)
            {
                throw new ArgumentException("RelatedPartyType must be 1 (Company) or 2 (Person).", nameof(stakeholder));
            }

            // Validate NoSelfCompany: if RelatedPartyType is 1 (Company), RelatedPartyId cannot equal CompanyId
            if (stakeholder.RelatedPartyType == 1 && stakeholder.RelatedPartyId == stakeholder.CompanyId)
            {
                throw new ArgumentException("A company cannot be a stakeholder of itself.", nameof(stakeholder));
            }
        }
    }
}
