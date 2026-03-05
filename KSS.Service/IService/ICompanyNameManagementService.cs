using KSS.Dto;
using KSS.Entity;
using KSS.Helper;

namespace KSS.Service.IService
{
    /// <summary>
    /// ManagementService for company name operations that span multiple tables:
    /// CompanyNameHistory, CompanyNameHistoryTranslation, and CompanyTranslation.
    ///
    /// The "ManagementService" suffix indicates this service orchestrates across multiple tables.
    /// Single-table CRUD is handled by the individual table services.
    /// </summary>
    public interface ICompanyNameManagementService
    {
        /// <summary>
        /// Add a new name history entry with all translations.
        /// Returns Fail if business rules are violated (e.g., StartDate before existing).
        /// </summary>
        Task<ServiceResult> AddNameWithTranslationsAsync(AddNameWithTranslationsDto dto);

        /// <summary>
        /// Upsert translations for an existing name history entry.
        /// Determines add vs update per language, syncs to CompanyTranslation.
        /// </summary>
        Task UpsertTranslationsAsync(UpsertNameTranslationsDto dto);

        /// <summary>
        /// Delete a name history entry with business rule validation.
        /// Returns Fail if: only entry, first entry, or not the newest entry.
        /// </summary>
        ServiceResult DeleteNameHistory(CompanyNameHistory item);

        /// <summary>
        /// Remove a single translation from a name history entry.
        /// Returns Fail if it's the last translation (every entry must have at least one).
        /// </summary>
        ServiceResult RemoveTranslation(RemoveTranslationDto dto);
    }
}
