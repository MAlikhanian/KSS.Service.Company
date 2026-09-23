using System.Security.Claims;
using AutoMapper;
using KSS.Helper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Service
{
    /// <summary>
    /// Adds the first name history entry of a company being created in the same operation.
    /// Internal to this assembly, so no controller can reach it.
    /// </summary>
    internal interface INewCompanyNameHistory
    {
        Task AddForNewCompanyAsync(NameHistory item);
    }

    public class NameHistoryService : BaseService<NameHistory, NameHistoryDto, NameHistoryInsertDto, NameHistoryDto>, INameHistoryService, INewCompanyNameHistory, ICompanyScopedWrites
    {
        private const int ModifyLevel = 2;
        private const string ModifyDenied = "You do not have permission to modify this company's name history.";
        private const string MoveDenied = "A name history entry cannot be moved to another company.";
        private const string NotThisCompany = "Name history entry not found for this company.";

        private readonly INameHistoryRepository _nameHistoryRepository;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NameHistoryService(
            IMapper mapper,
            INameHistoryRepository repository,
            IAccessService accessService,
            IHttpContextAccessor httpContextAccessor) : base(mapper, repository)
        {
            _nameHistoryRepository = repository;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
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
        public ServiceResult DeleteNameHistory(Guid id, Guid companyId)
        {
            var allEntries = _nameHistoryRepository.ToList(h => h.CompanyId == companyId);
            if (allEntries.Count() <= 1)
            {
                return ServiceResult.Fail("Cannot delete the only name history entry. Every company must have at least one name.");
            }

            var ordered = allEntries.OrderBy(h => h.StartDate).ToList();
            var earliest = ordered.First();
            var latest = ordered.Last();
            var trackedEntity = allEntries.FirstOrDefault(h => h.Id == id);
            if (trackedEntity == null)
            {
                return ServiceResult.Fail("Name history entry not found for this company.");
            }

            if (earliest.Id == id)
            {
                return ServiceResult.Fail("Cannot delete the original (first) name history entry for this company.");
            }

            if (latest.Id != id)
            {
                return ServiceResult.Fail("You can only delete the most recent name. Delete newer names first to avoid date gaps.");
            }

            // Reopen the previous entry — clear its EndDate so it becomes the current name again
            var previousEntry = ordered[ordered.Count - 2];
            previousEntry.EndDate = null;
            base.Update(previousEntry, saveChanges: false);

            // Use the already-tracked entity from the query.
            base.Remove(trackedEntity);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Add a new name history entry (entity) with business rule validation.
        /// Returns Fail if StartDate is before existing entries.
        /// </summary>
        public async Task<ServiceResult> AddNameHistoryAsync(NameHistory item, bool saveChanges = true)
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
        public async Task<ServiceResult> AddNameHistoryDtoAsync(NameHistoryDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<NameHistory>(item);
            ValidateNameHistory(entity);

            var error = CheckStartDateNotBeforeExisting(entity.CompanyId, entity.StartDate);
            if (error != null) return ServiceResult.Fail(error);

            ClosePreviousCurrentEntry(entity.CompanyId, entity.EndDate, entity.StartDate);
            await base.AddAsync(entity, saveChanges);
            return ServiceResult.Ok();
        }

        // ── Base overrides (safety net — delegates to result-returning methods above) ──
        // Every write reachable through BaseController checks Information level 2 on the
        // company its rows belong to. Updates and removals use the STORED row, never the
        // company in the request body. Range operations check every item before anything
        // is written: one denied item denies the whole call.

        public override void Remove(NameHistory item, bool saveChanges = true)
        {
            var stored = _nameHistoryRepository.Find(item.Id);
            if (stored == null || stored.CompanyId != item.CompanyId)
                throw new BusinessRuleException(NotThisCompany);
            RequireNameHistoryModifyAsync(stored.CompanyId).GetAwaiter().GetResult();

            var result = DeleteNameHistory(stored.Id, stored.CompanyId);
            if (!result.Success)
                throw new BusinessRuleException(result.Message!);
        }

        public override void RemoveRange(IEnumerable<NameHistory> items, bool saveChanges = true)
        {
            var stored = items
                .Select(x => _nameHistoryRepository.Find(x.Id))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            RequireModifyOnEveryCompanyAsync(stored.Select(x => x.CompanyId)).GetAwaiter().GetResult();
            // The tracked stored rows are removed, not the request instances.
            base.RemoveRange(stored, saveChanges);
        }

        public override async Task AddAsync(NameHistory item, bool saveChanges = true)
        {
            await RequireNameHistoryModifyAsync(item.CompanyId);
            await AddWithRulesAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(NameHistoryInsertDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<NameHistory>(item);
            await RequireNameHistoryModifyAsync(entity.CompanyId);
            await AddWithRulesAsync(entity, saveChanges);
        }

        public override async Task AddRangeAsync(IEnumerable<NameHistory> items, bool saveChanges = true)
        {
            var list = items.ToList();
            await RequireModifyOnEveryCompanyAsync(list.Select(x => x.CompanyId));
            await base.AddRangeAsync(list, saveChanges);
        }

        // Company creation adds the first entry of a company whose id is generated by the
        // server in the same operation, so there is no existing company to protect.
        Task INewCompanyNameHistory.AddForNewCompanyAsync(NameHistory item) => AddWithRulesAsync(item, true);

        private async Task AddWithRulesAsync(NameHistory item, bool saveChanges)
        {
            var result = await AddNameHistoryAsync(item, saveChanges);
            if (!result.Success)
                throw new BusinessRuleException(result.Message!);
        }

        // ── Update overrides (keep ArgumentException — standard .NET validation) ──

        public override void Update(NameHistory item, bool saveChanges = true)
        {
            var stored = PrepareUpdates(new[] { item });
            base.Update(stored[0], saveChanges);
        }

        public override void UpdateRange(IEnumerable<NameHistory> items, bool saveChanges = true)
        {
            base.UpdateRange(PrepareUpdates(items), saveChanges);
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// This avoids DbUpdateConcurrencyException caused by AutoMapper setting
        /// CreatedAt/UpdatedAt to default values on a detached entity.
        /// </summary>
        public override void UpdateDto(NameHistoryDto item, bool saveChanges = true)
        {
            var existing = _nameHistoryRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"NameHistory with Id '{item.Id}' not found.");

            // The stored row's company decides; the entry cannot be moved to another company.
            RequireNameHistoryModifyAsync(existing.CompanyId).GetAwaiter().GetResult();
            if (item.CompanyId != Guid.Empty && item.CompanyId != existing.CompanyId)
                throw new BusinessRuleException(MoveDenied);

            // Only update the editable fields - preserve CreatedAt, UpdatedAt (managed by trigger)
            existing.StartDate = item.StartDate;
            existing.EndDate = item.EndDate;
            existing.Description = item.Description;

            ValidateNameHistory(existing);
            base.Update(existing, saveChanges);
        }

        // ── Private helpers ──

        private static void ValidateNameHistory(NameHistory history)
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
        /// This satisfies the UX_NameHistory_Current unique filtered index.
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

        // Loads every stored row, checks all of them (company level on the stored row, and
        // no change of company) before any value is applied, then copies the request's
        // values onto the tracked stored rows. Writing through the tracked instance avoids
        // attaching a second instance with the same key.
        private List<NameHistory> PrepareUpdates(IEnumerable<NameHistory> items)
        {
            var pairs = items
                .Select(item => (Item: item, Stored: _nameHistoryRepository.Find(item.Id)
                    ?? throw new KeyNotFoundException($"NameHistory with Id '{item.Id}' not found.")))
                .ToList();

            if (pairs.Any(p => p.Item.CompanyId != p.Stored.CompanyId))
                throw new BusinessRuleException(MoveDenied);

            RequireModifyOnEveryCompanyAsync(pairs.Select(p => p.Stored.CompanyId)).GetAwaiter().GetResult();

            foreach (var (item, stored) in pairs)
            {
                CopyScalarValues(item, stored);
                ValidateNameHistory(stored);
            }

            return pairs.Select(p => p.Stored).ToList();
        }

        // Scalar columns only; navigation properties and the key are left untouched.
        private static void CopyScalarValues(NameHistory source, NameHistory target)
        {
            foreach (var property in typeof(NameHistory).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite || property.Name == nameof(NameHistory.Id))
                    continue;

                var type = property.PropertyType;
                if (type.IsValueType || type == typeof(string))
                    property.SetValue(target, property.GetValue(source));
            }
        }

        private async Task RequireModifyOnEveryCompanyAsync(IEnumerable<Guid> companyIds)
        {
            foreach (var companyId in companyIds.Distinct())
                await RequireNameHistoryModifyAsync(companyId);
        }

        // The Information.Modify permission (checked by the permission filter) is global to
        // the caller. This confirms the caller also holds Information level 2 on the
        // specific company the rows belong to. Fails closed: no caller, or a lower level,
        // is denied.
        private async Task RequireNameHistoryModifyAsync(Guid companyId)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, GetCallerPersonId());
            if (levels.Information < ModifyLevel)
                throw new BusinessRuleException(ModifyDenied);
        }

        private Guid GetCallerPersonId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var raw = user?.FindFirstValue("personId")
                   ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var personId))
                throw new BusinessRuleException("Caller PersonId not found on the JWT.");
            return personId;
        }
    }
}
