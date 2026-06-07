using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyStakeholderManagementService
    {
        /// <summary>
        /// Returns all stakeholders for the company (with full history per row).
        /// Returns null when the caller lacks row-level Information access.
        /// </summary>
        Task<List<CompanyStakeholderViewDto>?> GetByCompanyAsync(Guid companyId, Guid callerPersonId, short languageId = 12);

        /// <summary>
        /// Creates a stakeholder plus its initial (current) history row in a single transaction.
        /// </summary>
        Task<CompanyStakeholderViewDto> AddAsync(Guid companyId, Guid callerPersonId, CompanyStakeholderUpsertDto dto, short languageId = 12);

        /// <summary>
        /// Updates the stakeholder. If any history-shaped field differs from the
        /// current history row, closes that row (EndDate = today) and appends a
        /// new current row.
        /// </summary>
        Task<CompanyStakeholderViewDto> UpdateAsync(Guid stakeholderId, Guid callerPersonId, CompanyStakeholderUpsertDto dto, short languageId = 12);

        /// <summary>
        /// Soft-deletes the stakeholder and all its history rows.
        /// </summary>
        Task DeleteAsync(Guid stakeholderId, Guid callerPersonId);
    }
}
