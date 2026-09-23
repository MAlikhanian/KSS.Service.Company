using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class SoftwareCategoryTranslationService : BaseService<SoftwareCategoryTranslation, SoftwareCategoryTranslationDto, SoftwareCategoryTranslationDto, SoftwareCategoryTranslationDto>, ISoftwareCategoryTranslationService
    {
        public SoftwareCategoryTranslationService(IMapper mapper, ISoftwareCategoryTranslationRepository repository) : base(mapper, repository) { }
    }
}
