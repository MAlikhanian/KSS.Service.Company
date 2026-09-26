using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    // Reference data shared by every company: no row is a company's own record, so the generic reads stay open.
    public class PhoneLabelTranslationService : BaseService<PhoneLabelTranslation, PhoneLabelTranslationDto, PhoneLabelTranslationDto, PhoneLabelTranslationDto>, IPhoneLabelTranslationService, IReferenceData
    {
        public PhoneLabelTranslationService(IMapper mapper, IPhoneLabelTranslationRepository repository) : base(mapper, repository) { }
    }
}
