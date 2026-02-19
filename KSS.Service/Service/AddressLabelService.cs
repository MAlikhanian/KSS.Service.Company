using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class AddressLabelService : BaseService<AddressLabel, AddressLabelDto, AddressLabelDto, AddressLabelDto>, IAddressLabelService
    {
        public AddressLabelService(IMapper mapper, IAddressLabelRepository repository) : base(mapper, repository) { }
    }
}
