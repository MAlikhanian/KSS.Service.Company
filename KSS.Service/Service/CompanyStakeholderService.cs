using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyStakeholderService : BaseService<CompanyStakeholder, CompanyStakeholderDto, CompanyStakeholderDto, CompanyStakeholderDto>, ICompanyStakeholderService
    {
        public CompanyStakeholderService(IMapper mapper, ICompanyStakeholderRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(CompanyStakeholder item, bool saveChanges = true)
        {
            ValidateStakeholder(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(CompanyStakeholderDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyStakeholder>(item);
            ValidateStakeholder(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(CompanyStakeholder item, bool saveChanges = true)
        {
            ValidateStakeholder(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(CompanyStakeholderDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyStakeholder>(item);
            ValidateStakeholder(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidateStakeholder(CompanyStakeholder stakeholder)
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
