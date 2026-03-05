using KSS.Helper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    /// <summary>
    /// ManagementService that orchestrates company name operations across multiple tables:
    /// - CompanyNameHistory (via ICompanyNameHistoryService)
    /// - CompanyNameHistoryTranslation (via ICompanyNameHistoryTranslationService + repository)
    /// - CompanyTranslation (via ICompanyTranslationRepository)
    ///
    /// Single-table CRUD remains in the individual services.
    /// This service handles cross-table sync and multi-step business logic.
    /// </summary>
    public class CompanyNameManagementService : ICompanyNameManagementService
    {
        private readonly ICompanyNameHistoryService _nameHistoryService;
        private readonly ICompanyNameHistoryTranslationService _translationService;
        private readonly ICompanyNameHistoryTranslationRepository _translationRepository;
        private readonly ICompanyNameHistoryRepository _nameHistoryRepository;
        private readonly ICompanyTranslationRepository _companyTranslationRepository;

        public CompanyNameManagementService(
            ICompanyNameHistoryService nameHistoryService,
            ICompanyNameHistoryTranslationService translationService,
            ICompanyNameHistoryTranslationRepository translationRepository,
            ICompanyNameHistoryRepository nameHistoryRepository,
            ICompanyTranslationRepository companyTranslationRepository)
        {
            _nameHistoryService = nameHistoryService;
            _translationService = translationService;
            _translationRepository = translationRepository;
            _nameHistoryRepository = nameHistoryRepository;
            _companyTranslationRepository = companyTranslationRepository;
        }

        /// <summary>
        /// Add a new name history entry with all translations in one operation.
        /// 1) Creates the name history record (single-table service handles closing previous current)
        /// 2) Creates each translation
        /// 3) Syncs to CompanyTranslation if this is the current name
        /// </summary>
        public async Task<ServiceResult> AddNameWithTranslationsAsync(AddNameWithTranslationsDto dto)
        {
            // 1) Create the name history record — check result for business rule violations
            var nameHistoryDto = new CompanyNameHistoryDto
            {
                Id = dto.Id,
                CompanyId = dto.CompanyId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Description = dto.Description,
            };
            var result = await _nameHistoryService.AddNameHistoryDtoAsync(nameHistoryDto);
            if (!result.Success) return result;

            // 2) Create each translation (single-table CRUD only)
            foreach (var tr in dto.Translations)
            {
                if (!string.IsNullOrWhiteSpace(tr.Name))
                {
                    tr.CompanyNameHistoryId = dto.Id;
                    await _translationService.AddDtoAsync(tr);
                }
            }

            // 3) Sync CompanyTranslation with the current name (always, to stay consistent)
            SyncCurrentNameToCompanyTranslation(dto.CompanyId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Upsert translations for an existing name history entry.
        /// Determines add vs update per language, then syncs to CompanyTranslation.
        /// </summary>
        public async Task UpsertTranslationsAsync(UpsertNameTranslationsDto dto)
        {
            // Get existing translations for this name history entry
            var existing = _translationRepository.ToList(
                t => t.CompanyNameHistoryId == dto.NameHistoryId);
            var existingLangIds = new HashSet<short>(existing.Select(t => t.LanguageId));

            foreach (var tr in dto.Translations)
            {
                if (string.IsNullOrWhiteSpace(tr.Name)) continue;

                tr.CompanyNameHistoryId = dto.NameHistoryId;

                if (existingLangIds.Contains(tr.LanguageId))
                {
                    _translationService.UpdateDto(tr);
                }
                else
                {
                    await _translationService.AddDtoAsync(tr);
                }
            }

            // Sync CompanyTranslation with the current name (always, to stay consistent)
            var nameHistory = _nameHistoryRepository.SingleOrDefault(
                h => h.Id == dto.NameHistoryId);

            if (nameHistory != null)
            {
                SyncCurrentNameToCompanyTranslation(nameHistory.CompanyId);
            }
        }

        /// <summary>
        /// Delete a name history entry and sync CompanyTranslation with the new current name.
        /// After delete, the previous entry becomes current — its translations must be synced.
        /// </summary>
        public ServiceResult DeleteNameHistory(CompanyNameHistory item)
        {
            var result = _nameHistoryService.DeleteNameHistory(item);
            if (!result.Success) return result;

            // After delete, the previous entry is now current (EndDate was cleared).
            // Sync its translations to CompanyTranslation.
            SyncCurrentNameToCompanyTranslation(item.CompanyId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Remove a single translation from a name history entry.
        /// Returns Fail if it's the last translation. Removes from CompanyTranslation if current.
        /// </summary>
        public ServiceResult RemoveTranslation(RemoveTranslationDto dto)
        {
            // Prevent deleting the last translation
            var translationCount = _translationRepository.Count(
                t => t.CompanyNameHistoryId == dto.CompanyNameHistoryId);
            if (translationCount <= 1)
            {
                return ServiceResult.Fail(
                    "Cannot delete the last translation. Every name history entry must have at least one translation.");
            }

            // Find and remove the translation entity
            var entity = _translationRepository.SingleOrDefault(
                t => t.CompanyNameHistoryId == dto.CompanyNameHistoryId && t.LanguageId == dto.LanguageId);

            if (entity == null)
            {
                return ServiceResult.Fail(
                    $"Translation for language {dto.LanguageId} not found on this name history entry.");
            }

            _translationService.Remove(entity);

            // If this name history is the current one, also remove from CompanyTranslation
            var nameHistory = _nameHistoryRepository.SingleOrDefault(
                h => h.Id == dto.CompanyNameHistoryId);

            if (nameHistory != null && nameHistory.EndDate == null)
            {
                var companyTranslation = _companyTranslationRepository.SingleOrDefault(
                    t => t.CompanyId == nameHistory.CompanyId && t.LanguageId == dto.LanguageId);

                if (companyTranslation != null)
                {
                    _companyTranslationRepository.Remove(companyTranslation);
                }
            }

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Sync CompanyTranslation with the current name history entry's translations.
        /// Finds the current entry (EndDate IS NULL), reads all its translations,
        /// and upserts each one into CompanyTranslation.
        /// </summary>
        private void SyncCurrentNameToCompanyTranslation(Guid companyId)
        {
            // Find the current name history entry
            var currentEntry = _nameHistoryRepository.SingleOrDefault(
                h => h.CompanyId == companyId && h.EndDate == null);

            if (currentEntry == null) return;

            // Get all translations for the current entry
            var translations = _translationRepository.ToList(
                t => t.CompanyNameHistoryId == currentEntry.Id);

            foreach (var tr in translations)
            {
                SyncToCompanyTranslation(companyId, tr.LanguageId, tr.Name, tr.ShortName);
            }
        }

        /// <summary>
        /// Upsert the CompanyTranslation so the company's primary name stays in sync
        /// with the current name history entry.
        /// </summary>
        private void SyncToCompanyTranslation(Guid companyId, short languageId, string name, string? shortName)
        {
            var existing = _companyTranslationRepository.SingleOrDefault(
                t => t.CompanyId == companyId && t.LanguageId == languageId);

            if (existing != null)
            {
                existing.Name = name;
                existing.ShortName = shortName;
                _companyTranslationRepository.Update(existing);
            }
            else
            {
                var newTranslation = new CompanyTranslation
                {
                    CompanyId = companyId,
                    LanguageId = languageId,
                    Name = name,
                    ShortName = shortName,
                };
                _companyTranslationRepository.Add(newTranslation);
            }
        }
    }
}
