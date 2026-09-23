using System.Security.Claims;
using KSS.Helper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Service
{
    /// <summary>
    /// ManagementService that orchestrates company name operations across multiple tables:
    /// - NameHistory (via INameHistoryService)
    /// - NameHistoryTranslation (via INameHistoryTranslationService + repository)
    /// - Translation (via ITranslationRepository)
    ///
    /// Single-table CRUD remains in the individual services.
    /// This service handles cross-table sync and multi-step business logic.
    /// </summary>
    public class CompanyNameManagementService : ICompanyNameManagementService
    {
        private const int ModifyLevel = 2;
        private const string ModifyDenied = "You do not have permission to modify this company's names.";
        private const string NotThisCompany = "Name history entry not found for this company.";
        private const string NameHistoryNotFound = "Name history entry not found.";

        private readonly INameHistoryService _nameHistoryService;
        private readonly INameHistoryTranslationService _translationService;
        private readonly INameHistoryTranslationRepository _translationRepository;
        private readonly INameHistoryRepository _nameHistoryRepository;
        private readonly ITranslationRepository _companyTranslationRepository;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyNameManagementService(
            INameHistoryService nameHistoryService,
            INameHistoryTranslationService translationService,
            INameHistoryTranslationRepository translationRepository,
            INameHistoryRepository nameHistoryRepository,
            ITranslationRepository companyTranslationRepository,
            IAccessService accessService,
            IHttpContextAccessor httpContextAccessor)
        {
            _nameHistoryService = nameHistoryService;
            _translationService = translationService;
            _translationRepository = translationRepository;
            _nameHistoryRepository = nameHistoryRepository;
            _companyTranslationRepository = companyTranslationRepository;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Add a new name history entry with all translations in one operation.
        /// 1) Creates the name history record (single-table service handles closing previous current)
        /// 2) Creates each translation
        /// 3) Syncs to Translation if this is the current name
        /// </summary>
        public async Task<ServiceResult> AddNameWithTranslationsAsync(AddNameWithTranslationsDto dto)
        {
            await RequireCompanyNameModifyAsync(dto.CompanyId);

            // 1) Create the name history record — id generated here (v7) because
            // the translations below reference it as a FK in the same operation.
            var nameHistoryId = Guid.CreateVersion7();
            var nameHistoryDto = new NameHistoryDto
            {
                Id = nameHistoryId,
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
                    await _translationService.AddDtoAsync(new NameHistoryTranslationDto
                    {
                        NameHistoryId = nameHistoryId,
                        LanguageId = tr.LanguageId,
                        Name = tr.Name,
                        ShortName = tr.ShortName,
                    });
                }
            }

            // 3) Sync Translation with the current name (always, to stay consistent)
            SyncCurrentNameToCompanyTranslation(dto.CompanyId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Upsert translations for an existing name history entry.
        /// Determines add vs update per language, then syncs to Translation.
        /// </summary>
        public async Task UpsertTranslationsAsync(UpsertNameTranslationsDto dto)
        {
            // The stored name history entry decides the company. It is resolved and
            // checked before any translation is written.
            var nameHistory = _nameHistoryRepository.SingleOrDefault(
                h => h.Id == dto.NameHistoryId)
                ?? throw new KeyNotFoundException(NameHistoryNotFound);
            await RequireCompanyNameModifyAsync(nameHistory.CompanyId);
            if (dto.CompanyId != Guid.Empty && dto.CompanyId != nameHistory.CompanyId)
                throw new BusinessRuleException(NotThisCompany);

            // Get existing translations for this name history entry
            var existing = _translationRepository.ToList(
                t => t.NameHistoryId == dto.NameHistoryId);
            var existingLangIds = new HashSet<short>(existing.Select(t => t.LanguageId));

            foreach (var tr in dto.Translations)
            {
                if (string.IsNullOrWhiteSpace(tr.Name)) continue;

                tr.NameHistoryId = dto.NameHistoryId;

                if (existingLangIds.Contains(tr.LanguageId))
                {
                    _translationService.UpdateDto(tr);
                }
                else
                {
                    await _translationService.AddDtoAsync(tr);
                }
            }

            // Sync Translation with the current name (always, to stay consistent)
            SyncCurrentNameToCompanyTranslation(nameHistory.CompanyId);
        }

        /// <summary>
        /// Delete a name history entry and sync Translation with the new current name.
        /// After delete, the previous entry becomes current — its translations must be synced.
        /// </summary>
        public ServiceResult DeleteNameHistory(Guid id, Guid companyId)
        {
            // The stored entry decides the company; an entry of another company is
            // treated as not found for the one requested, as the delete rule already does.
            var stored = _nameHistoryRepository.SingleOrDefault(h => h.Id == id);
            if (stored == null || stored.CompanyId != companyId)
                return ServiceResult.Fail(NotThisCompany);
            RequireCompanyNameModifyAsync(stored.CompanyId).GetAwaiter().GetResult();

            var result = _nameHistoryService.DeleteNameHistory(id, stored.CompanyId);
            if (!result.Success) return result;

            // After delete, the previous entry is now current (EndDate was cleared).
            // Sync its translations to Translation.
            SyncCurrentNameToCompanyTranslation(stored.CompanyId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Remove a single translation from a name history entry.
        /// Returns Fail if it's the last translation. Removes from Translation if current.
        /// </summary>
        public ServiceResult RemoveTranslation(RemoveTranslationDto dto)
        {
            // The stored name history entry decides the company. It is resolved and
            // checked before anything is removed.
            var nameHistory = _nameHistoryRepository.SingleOrDefault(
                h => h.Id == dto.NameHistoryId);
            if (nameHistory == null)
                return ServiceResult.Fail(NameHistoryNotFound);
            RequireCompanyNameModifyAsync(nameHistory.CompanyId).GetAwaiter().GetResult();

            // Prevent deleting the last translation
            var translationCount = _translationRepository.Count(
                t => t.NameHistoryId == dto.NameHistoryId);
            if (translationCount <= 1)
            {
                return ServiceResult.Fail(
                    "Cannot delete the last translation. Every name history entry must have at least one translation.");
            }

            // Find and remove the translation entity
            var entity = _translationRepository.SingleOrDefault(
                t => t.NameHistoryId == dto.NameHistoryId && t.LanguageId == dto.LanguageId);

            if (entity == null)
            {
                return ServiceResult.Fail(
                    $"Translation for language {dto.LanguageId} not found on this name history entry.");
            }

            _translationService.Remove(entity);

            // If this name history is the current one, also remove from Translation
            if (nameHistory.EndDate == null)
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
        /// Sync Translation with the current name history entry's translations.
        /// Finds the current entry (EndDate IS NULL), reads all its translations,
        /// and upserts each one into Translation.
        /// </summary>
        private void SyncCurrentNameToCompanyTranslation(Guid companyId)
        {
            // Find the current name history entry
            var currentEntry = _nameHistoryRepository.SingleOrDefault(
                h => h.CompanyId == companyId && h.EndDate == null);

            if (currentEntry == null) return;

            // Get all translations for the current entry
            var translations = _translationRepository.ToList(
                t => t.NameHistoryId == currentEntry.Id);

            foreach (var tr in translations)
            {
                SyncToCompanyTranslation(companyId, tr.LanguageId, tr.Name, tr.ShortName);
            }
        }

        /// <summary>
        /// Upsert the Translation so the company's primary name stays in sync
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
                var newTranslation = new Translation
                {
                    CompanyId = companyId,
                    LanguageId = languageId,
                    Name = name,
                    ShortName = shortName,
                };
                _companyTranslationRepository.Add(newTranslation);
            }
        }

        // The Information.Modify permission (checked by the controller attribute) is
        // global to the caller. This confirms the caller also holds Information level 2
        // on the specific company being changed. Fails closed: no caller, or a lower
        // level, is denied.
        private async Task RequireCompanyNameModifyAsync(Guid companyId)
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
