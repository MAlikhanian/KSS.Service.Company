using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyNameHistoryService : BaseService<CompanyNameHistory, CompanyNameHistoryDto, CompanyNameHistoryDto, CompanyNameHistoryDto>, ICompanyNameHistoryService
    {
        public CompanyNameHistoryService(IMapper mapper, ICompanyNameHistoryRepository repository) : base(mapper, repository) { }

        public override async Task AddAsync(CompanyNameHistory item, bool saveChanges = true)
        {
            ValidateNameHistory(item);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(CompanyNameHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyNameHistory>(item);
            ValidateNameHistory(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override void Update(CompanyNameHistory item, bool saveChanges = true)
        {
            ValidateNameHistory(item);
            base.Update(item, saveChanges);
        }

        public override void UpdateDto(CompanyNameHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyNameHistory>(item);
            ValidateNameHistory(entity);
            base.Update(entity, saveChanges);
        }

        private static void ValidateNameHistory(CompanyNameHistory history)
        {
            // Validate DateRange: if EndDate is not null, StartDate must be <= EndDate
            if (history.EndDate.HasValue && history.StartDate > history.EndDate.Value)
            {
                throw new ArgumentException("StartDate must be less than or equal to EndDate.", nameof(history));
            }
        }
    }
}
