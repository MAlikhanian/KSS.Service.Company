using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyDetailService
    {
        /// <summary>
        /// Returns the company detail when the caller has Information access (>=1).
        /// Returns null when the company does not exist OR when the caller lacks
        /// row-level access — controllers should map null to 404 to avoid leaking
        /// existence to unauthorized callers.
        /// </summary>
        Task<CompanyDetailDto?> GetByIdAsync(Guid id, Guid callerPersonId, short languageId = 12);

        /// <summary>
        /// Updates the company. Throws BusinessRuleException when the caller lacks
        /// Information.Modify access (level &lt; 2) for this company.
        /// </summary>
        Task<CompanyDetailDto> UpdateAsync(Guid id, Guid callerPersonId, CompanyDetailDto dto);
    }
}
