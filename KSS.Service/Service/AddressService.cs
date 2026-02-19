using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class AddressService : BaseService<Address, AddressDto, AddressDto, AddressDto>, IAddressService
    {
        public AddressService(IMapper mapper, IAddressRepository repository) : base(mapper, repository) { }
    }
}
