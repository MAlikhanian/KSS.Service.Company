using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class TranslationService : BaseService<Translation, TranslationDto, TranslationDto, TranslationDto>, ITranslationService
    {
        private readonly ITranslationRepository _translationRepository;

        public TranslationService(IMapper mapper, ITranslationRepository repository) : base(mapper, repository)
        {
            _translationRepository = repository;
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// Prevents DbUpdateConcurrencyException from _dbSet.Update() on detached entity
        /// and avoids overwriting ShortName/Description with null when not provided.
        /// </summary>
        public override void UpdateDto(TranslationDto item, bool saveChanges = true)
        {
            var existing = _translationRepository.SingleOrDefault(
                t => t.CompanyId == item.CompanyId && t.LanguageId == item.LanguageId)
                ?? throw new KeyNotFoundException(
                    $"Translation with key ({item.CompanyId}, {item.LanguageId}) not found.");

            existing.Name = item.Name;
            existing.ShortName = item.ShortName;
            existing.Description = item.Description;

            base.Update(existing, saveChanges);
        }
    }
}
