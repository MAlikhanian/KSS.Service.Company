using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyStakeholderHistoryService : BaseService<CompanyStakeholderHistory, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto, CompanyStakeholderHistoryDto>, ICompanyStakeholderHistoryService
    {
        public CompanyStakeholderHistoryService(IMapper mapper, ICompanyStakeholderHistoryRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(CompanyStakeholderHistory item, bool saveChanges = true)
        {
            ValidateStakeholderHistory(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(CompanyStakeholderHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyStakeholderHistory>(item);
            ValidateStakeholderHistory(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(CompanyStakeholderHistory item, bool saveChanges = true)
        {
            ValidateStakeholderHistory(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(CompanyStakeholderHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyStakeholderHistory>(item);
            ValidateStakeholderHistory(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidateStakeholderHistory(CompanyStakeholderHistory history)
        {
            // Validate OwnershipPercentage: must be between 0 and 100
            if (history.OwnershipPercentage < 0 || history.OwnershipPercentage > 100)
            {
                throw new ArgumentException("OwnershipPercentage must be between 0 and 100.", nameof(history));
            }

            // Validate ShareCount: must be >= 0
            if (history.ShareCount < 0)
            {
                throw new ArgumentException("ShareCount must be greater than or equal to 0.", nameof(history));
            }

            // Validate DateRange: if EndDate is not null, EffectiveDate must be <= EndDate
            if (history.EndDate.HasValue && history.EffectiveDate > history.EndDate.Value)
            {
                throw new ArgumentException("EffectiveDate must be less than or equal to EndDate.", nameof(history));
            }
        }
    }
}
