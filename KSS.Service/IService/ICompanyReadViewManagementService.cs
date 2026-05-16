using KSS.Dto;

namespace KSS.Service.IService
{
    /// <summary>
    /// ManagementService that builds a read-only consolidated view of a company
    /// for display screens that need company core data + name history + contacts
    /// in a single response.
    ///
    /// The "ManagementService" suffix indicates this service spans multiple tables:
    /// Company, Translation, NameHistory, NameHistoryTranslation, Email,
    /// EmailLabelTranslation, Phone, PhoneLabelTranslation, Address,
    /// AddressLabelTranslation, AddressTranslation.
    /// </summary>
    public interface ICompanyReadViewManagementService
    {
        /// <summary>
        /// Get the consolidated read-only view for a company. Returns null when
        /// the company does not exist OR when the caller lacks row-level access
        /// (Information &lt; 1) — controllers should map null to 404.
        /// </summary>
        Task<CompanyReadViewDto?> GetByIdAsync(Guid companyId, Guid callerPersonId, short languageId = 12);
    }
}
