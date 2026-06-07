using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface IAccessService : IBaseService<Access, AccessDto, AccessAddDto, AccessDto>
    {
        /// <summary>
        /// Returns the caller's per-section levels for the given company.
        /// If the caller IS the company's owner (Company.CreatedBy == caller),
        /// returns {2,2}. Otherwise, sections with no row default to 0 (None).
        /// Role-based grants (RoleAccess) are layered in via MAX(personal, role).
        /// </summary>
        Task<AccessLevelsDto> GetLevelsAsync(Guid companyId, Guid callerPersonId);

        /// <summary>
        /// Owner-only. Replaces all access rows for (CompanyId, GrantedToPersonId)
        /// with one row per section whose Level &gt; 0.
        /// </summary>
        Task UpsertGrantAsync(AccessGrantDto dto, Guid callerPersonId);

        /// <summary>
        /// Owner-only. Deletes all rows for the (CompanyId, GrantedToPersonId) pair.
        /// </summary>
        Task RevokeByPairAsync(Guid companyId, Guid grantedToPersonId, Guid callerPersonId);

        /// <summary>
        /// Lists every grantee on a company, with both section levels rolled up.
        /// </summary>
        Task<List<AccessGrantSummaryDto>> ListGrantsByCompanyAsync(Guid companyId);

        /// <summary>
        /// Tenant-wide flat list of (CompanyId, GrantedToPersonId) pairs across
        /// every section. Used by cross-service dashboard report endpoints —
        /// returns only the two IDs, no section levels or timestamps.
        /// </summary>
        Task<List<AccessGrantPairDto>> ListAllGrantPairsAsync();
    }
}
