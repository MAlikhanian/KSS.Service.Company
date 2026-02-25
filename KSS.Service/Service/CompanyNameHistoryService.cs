using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyNameHistoryService : BaseService<CompanyNameHistory, CompanyNameHistoryDto, CompanyNameHistoryDto, CompanyNameHistoryDto>, ICompanyNameHistoryService
    {
        private readonly ICompanyNameHistoryRepository _nameHistoryRepository;

        public CompanyNameHistoryService(IMapper mapper, ICompanyNameHistoryRepository repository) : base(mapper, repository)
        {
            _nameHistoryRepository = repository;
        }

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

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// This avoids DbUpdateConcurrencyException caused by AutoMapper setting
        /// CreatedAt/UpdatedAt to default values on a detached entity.
        /// </summary>
        public override void UpdateDto(CompanyNameHistoryDto item, bool saveChanges = true)
        {
            var existing = _nameHistoryRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"CompanyNameHistory with Id '{item.Id}' not found.");

            // Only update the editable fields — preserve CreatedAt, UpdatedAt (managed by trigger)
            existing.CompanyId = item.CompanyId;
            existing.StartDate = item.StartDate;
            existing.EndDate = item.EndDate;
            existing.Description = item.Description;

            ValidateNameHistory(existing);
            base.Update(existing, saveChanges);
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
