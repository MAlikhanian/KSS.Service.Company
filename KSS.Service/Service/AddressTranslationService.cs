using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class AddressTranslationService : BaseService<AddressTranslation, AddressTranslationDto, AddressTranslationDto, AddressTranslationDto>, IAddressTranslationService
    {
        public AddressTranslationService(IMapper mapper, IAddressTranslationRepository repository) : base(mapper, repository) { }
    }
}
