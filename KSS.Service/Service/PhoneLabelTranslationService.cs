using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class PhoneLabelTranslationService : BaseService<PhoneLabelTranslation, PhoneLabelTranslationDto, PhoneLabelTranslationDto, PhoneLabelTranslationDto>, IPhoneLabelTranslationService
    {
        public PhoneLabelTranslationService(IMapper mapper, IPhoneLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
