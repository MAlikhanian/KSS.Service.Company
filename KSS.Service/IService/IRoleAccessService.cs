using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface IRoleAccessService : IBaseService<RoleAccess, RoleAccessDto, RoleAccessDto, RoleAccessDto>
    {
        /// <summary>
        /// Per-company role grants for a single company, plus all global grants.
        /// One entry per (CompanyId, GrantedToRoleId) pair — CompanyId may be null for globals.
        /// </summary>
        Task<List<RoleAccessGrantSummaryDto>> ListGrantsByCompanyAsync(Guid companyId);

        /// <summary>
        /// Every role grant in the system, grouped by (CompanyId, GrantedToRoleId).
        /// Used for the global admin view.
        /// </summary>
        Task<List<RoleAccessGrantSummaryDto>> ListAllGrantsAsync();

        /// <summary>
        /// Replaces all role-access rows for a (CompanyId, GrantedToRoleId) pair
        /// with one row per section whose Level &gt; 0. CompanyId == null means
        /// the grant applies to all companies.
        /// </summary>
        Task UpsertGrantAsync(RoleAccessGrantDto dto, Guid callerPersonId);

        /// <summary>
        /// Deletes all rows for a (CompanyId, GrantedToRoleId) pair. CompanyId may
        /// be null to revoke a global grant.
        /// </summary>
        Task RevokeByPairAsync(Guid? companyId, Guid grantedToRoleId, Guid callerPersonId);
    }
}
