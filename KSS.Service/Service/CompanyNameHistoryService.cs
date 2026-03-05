using AutoMapper;
using KSS.Helper;
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

        // ── New result-returning methods (no exceptions for business rules) ──

        /// <summary>
        /// Delete a name history entry with business rule validation.
        /// Rules:
        /// 1. Cannot delete the only entry (company must always have a name)
        /// 2. Cannot delete the first/original entry (earliest StartDate)
        /// 3. Can ONLY delete the newest entry (latest StartDate) — must delete newest-to-oldest
        /// 4. When the newest is deleted, the previous entry's EndDate is cleared (becomes current)
        /// </summary>
        public ServiceResult DeleteNameHistory(CompanyNameHistory item)
        {
            var allEntries = _nameHistoryRepository.ToList(h => h.CompanyId == item.CompanyId);
            if (allEntries.Count() <= 1)
            {
                return ServiceResult.Fail("Cannot delete the only name history entry. Every company must have at least one name.");
            }

            var ordered = allEntries.OrderBy(h => h.StartDate).ToList();
            var earliest = ordered.First();
            var latest = ordered.Last();
            var trackedEntity = allEntries.First(h => h.Id == item.Id);

            if (earliest.Id == item.Id)
            {
                return ServiceResult.Fail("Cannot delete the original (first) name history entry for this company.");
            }

            if (latest.Id != item.Id)
            {
                return ServiceResult.Fail("You can only delete the most recent name. Delete newer names first to avoid date gaps.");
            }

            // Reopen the previous entry — clear its EndDate so it becomes the current name again
            var previousEntry = ordered[ordered.Count - 2];
            previousEntry.EndDate = null;
            base.Update(previousEntry, saveChanges: false);

            // Use the already-tracked entity from the query, not the detached item from the controller
            base.Remove(trackedEntity);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Add a new name history entry (entity) with business rule validation.
        /// Returns Fail if StartDate is before existing entries.
        /// </summary>
        public async Task<ServiceResult> AddNameHistoryAsync(CompanyNameHistory item, bool saveChanges = true)
        {
            ValidateNameHistory(item);

            var error = CheckStartDateNotBeforeExisting(item.CompanyId, item.StartDate);
            if (error != null) return ServiceResult.Fail(error);

            ClosePreviousCurrentEntry(item.CompanyId, item.EndDate, item.StartDate);
            await base.AddAsync(item, saveChanges);
            return ServiceResult.Ok();
        }

        /// <summary>
        /// Add a new name history entry (DTO) with business rule validation.
        /// Returns Fail if StartDate is before existing entries.
        /// </summary>
        public async Task<ServiceResult> AddNameHistoryDtoAsync(CompanyNameHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<CompanyNameHistory>(item);
            ValidateNameHistory(entity);

            var error = CheckStartDateNotBeforeExisting(entity.CompanyId, entity.StartDate);
            if (error != null) return ServiceResult.Fail(error);

            ClosePreviousCurrentEntry(entity.CompanyId, entity.EndDate, entity.StartDate);
            await base.AddAsync(entity, saveChanges);
            return ServiceResult.Ok();
        }

        // ── Base overrides (safety net — delegates to result-returning methods above) ──

        public override void Remove(CompanyNameHistory item, bool saveChanges = true)
        {
            var result = DeleteNameHistory(item);
            if (!result.Success)
                throw new BusinessRuleException(result.Message!);
        }

        public override async Task AddAsync(CompanyNameHistory item, bool saveChanges = true)
        {
            var result = await AddNameHistoryAsync(item, saveChanges);
            if (!result.Success)
                throw new BusinessRuleException(result.Message!);
        }

        public override async Task AddDtoAsync(CompanyNameHistoryDto item, bool saveChanges = true)
        {
            var result = await AddNameHistoryDtoAsync(item, saveChanges);
            if (!result.Success)
                throw new BusinessRuleException(result.Message!);
        }

        // ── Update overrides (keep ArgumentException — standard .NET validation) ──

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

            // Only update the editable fields - preserve CreatedAt, UpdatedAt (managed by trigger)
            existing.CompanyId = item.CompanyId;
            existing.StartDate = item.StartDate;
            existing.EndDate = item.EndDate;
            existing.Description = item.Description;

            ValidateNameHistory(existing);
            base.Update(existing, saveChanges);
        }

        // ── Private helpers ──

        private static void ValidateNameHistory(CompanyNameHistory history)
        {
            // Validate DateRange: if EndDate is not null, StartDate must be <= EndDate
            if (history.EndDate.HasValue && history.StartDate > history.EndDate.Value)
            {
                throw new ArgumentException("StartDate must be less than or equal to EndDate.", nameof(history));
            }
        }

        /// <summary>
        /// Check that StartDate is not before existing entries.
        /// Returns null if valid, error message if invalid.
        /// </summary>
        private string? CheckStartDateNotBeforeExisting(Guid companyId, DateTime newStartDate)
        {
            var existingEntries = _nameHistoryRepository.ToList(h => h.CompanyId == companyId);
            if (!existingEntries.Any()) return null; // First entry for this company - no restriction

            var latestStartDate = existingEntries.Max(h => h.StartDate);
            if (newStartDate < latestStartDate)
            {
                return $"Cannot add a name with StartDate ({newStartDate:yyyy-MM-dd}) earlier than an existing name ({latestStartDate:yyyy-MM-dd}). New names must be chronologically after existing ones.";
            }
            return null;
        }

        /// <summary>
        /// When adding a new "current" entry (EndDate is null), close the previous current
        /// entry by setting its EndDate to the new entry's StartDate.
        /// This satisfies the UX_CompanyNameHistory_Current unique filtered index.
        /// </summary>
        private void ClosePreviousCurrentEntry(Guid companyId, DateTime? newEndDate, DateTime newStartDate)
        {
            if (newEndDate.HasValue) return; // Not a current entry - no need to close anything

            var previousCurrent = _nameHistoryRepository.SingleOrDefault(
                h => h.CompanyId == companyId && h.EndDate == null);

            if (previousCurrent != null)
            {
                previousCurrent.EndDate = newStartDate;
                base.Update(previousCurrent, saveChanges: false); // Save together with the new entry
            }
        }
    }
}
