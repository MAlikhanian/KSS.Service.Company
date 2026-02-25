using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyNameHistoryTranslationService : BaseService<CompanyNameHistoryTranslation, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto>, ICompanyNameHistoryTranslationService
    {
        private readonly ICompanyNameHistoryTranslationRepository _translationRepository;

        public CompanyNameHistoryTranslationService(IMapper mapper, ICompanyNameHistoryTranslationRepository repository) : base(mapper, repository)
        {
            _translationRepository = repository;
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// This avoids DbUpdateConcurrencyException caused by _dbSet.Update()
        /// on a detached entity that doesn't exist or has stale tracked state.
        /// </summary>
        public override void UpdateDto(CompanyNameHistoryTranslationDto item, bool saveChanges = true)
        {
            var existing = _translationRepository.SingleOrDefault(
                t => t.CompanyNameHistoryId == item.CompanyNameHistoryId && t.LanguageId == item.LanguageId)
                ?? throw new KeyNotFoundException(
                    $"CompanyNameHistoryTranslation with key ({item.CompanyNameHistoryId}, {item.LanguageId}) not found.");

            existing.Name = item.Name;
            existing.ShortName = item.ShortName;

            base.Update(existing, saveChanges);
        }
    }
}
