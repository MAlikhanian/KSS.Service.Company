using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface IAddressService : IBaseService<Address, AddressDto, AddressInsertDto, AddressDto>
    {
        /// <summary>A company's address rows (report read — no access filter).</summary>
        Task<List<AddressDto>> GetByCompanyAsync(Guid companyId);
    }
}
