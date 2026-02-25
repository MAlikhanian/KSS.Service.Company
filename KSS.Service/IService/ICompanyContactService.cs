using KSS.Dto;

namespace KSS.Service.IService
{
    public interface ICompanyContactService
    {
        Task<CompanyContactDto> GetContactDataAsync(Guid companyId, short languageId = 12);
        Task<CompanyEmailViewDto> AddEmailAsync(Guid companyId, CompanyEmailViewDto dto);
        Task<CompanyEmailViewDto> UpdateEmailAsync(Guid emailId, CompanyEmailViewDto dto);
        Task DeleteEmailAsync(Guid emailId);
        Task<CompanyPhoneViewDto> AddPhoneAsync(Guid companyId, CompanyPhoneViewDto dto);
        Task<CompanyPhoneViewDto> UpdatePhoneAsync(Guid phoneId, CompanyPhoneViewDto dto);
        Task DeletePhoneAsync(Guid phoneId);
        Task<CompanyAddressViewDto> AddAddressAsync(Guid companyId, CompanyAddressViewDto dto, short languageId = 12);
        Task<CompanyAddressViewDto> UpdateAddressAsync(Guid addressId, CompanyAddressViewDto dto, short languageId = 12);
        Task DeleteAddressAsync(Guid addressId);
    }
}
