using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyContactService
    {
        /// <summary>
        /// Returns the contact data when the caller has Information access (>=1).
        /// Returns null when the caller lacks row-level access — controllers
        /// should map null to 404 to avoid leaking existence.
        /// </summary>
        Task<CompanyContactDto?> GetContactDataAsync(Guid companyId, Guid callerPersonId, short languageId = 12);
        Task<CompanyEmailViewDto> AddEmailAsync(Guid companyId, CompanyEmailInsertDto dto);
        Task<CompanyEmailViewDto> UpdateEmailAsync(Guid emailId, CompanyEmailViewDto dto);
        Task DeleteEmailAsync(Guid emailId);
        Task<CompanyPhoneViewDto> AddPhoneAsync(Guid companyId, CompanyPhoneInsertDto dto);
        Task<CompanyPhoneViewDto> UpdatePhoneAsync(Guid phoneId, CompanyPhoneViewDto dto);
        Task DeletePhoneAsync(Guid phoneId);
        Task<CompanyAddressViewDto> AddAddressAsync(Guid companyId, CompanyAddressInsertDto dto, short languageId = 12);
        Task<CompanyAddressViewDto> UpdateAddressAsync(Guid addressId, CompanyAddressViewDto dto, short languageId = 12);
        Task DeleteAddressAsync(Guid addressId);
        Task<CompanyWebsiteViewDto> AddWebsiteAsync(Guid companyId, CompanyWebsiteInsertDto dto);
        Task<CompanyWebsiteViewDto> UpdateWebsiteAsync(Guid websiteId, CompanyWebsiteViewDto dto);
        Task DeleteWebsiteAsync(Guid websiteId);
    }
}
