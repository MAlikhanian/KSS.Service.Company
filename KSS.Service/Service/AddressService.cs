using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class AddressService : BaseService<Address, AddressDto, AddressInsertDto, AddressDto>, IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IMapper mapper, IAddressRepository repository) : base(mapper, repository)
        {
            _addressRepository = repository;
        }

        // Report read — a company's address rows, no per-caller access filter.
        public async Task<List<AddressDto>> GetByCompanyAsync(Guid companyId)
        {
            var rows = await _addressRepository.ToListAsync(x => x.CompanyId == companyId);
            return _mapper.Map<List<AddressDto>>(rows);
        }
    }
}
