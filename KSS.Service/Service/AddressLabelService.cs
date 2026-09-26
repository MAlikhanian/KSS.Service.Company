using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class AddressLabelService : BaseService<AddressLabel, AddressLabelDto, AddressLabelDto, AddressLabelDto>, IAddressLabelService, IReferenceData
    {
        public AddressLabelService(IMapper mapper, IAddressLabelRepository repository) : base(mapper, repository) { }
    }
}
