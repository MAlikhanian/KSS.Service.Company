using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class AddressLabelTranslationService : BaseService<AddressLabelTranslation, AddressLabelTranslationDto, AddressLabelTranslationDto, AddressLabelTranslationDto>, IAddressLabelTranslationService
    {
        public AddressLabelTranslationService(IMapper mapper, IAddressLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
