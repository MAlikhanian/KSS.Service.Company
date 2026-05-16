using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    /// <summary>
    /// Single-table CRUD service for NameHistoryTranslation.
    /// Multi-table orchestration (sync to Translation, etc.) is handled by CompanyNameManagementService.
    /// </summary>
    public class NameHistoryTranslationService : BaseService<NameHistoryTranslation, NameHistoryTranslationDto, NameHistoryTranslationDto, NameHistoryTranslationDto>, INameHistoryTranslationService
    {
        private readonly INameHistoryTranslationRepository _translationRepository;

        public NameHistoryTranslationService(
            IMapper mapper,
            INameHistoryTranslationRepository repository) : base(mapper, repository)
        {
            _translationRepository = repository;
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// Prevents DbUpdateConcurrencyException from _dbSet.Update() on detached entity.
        /// </summary>
        public override void UpdateDto(NameHistoryTranslationDto item, bool saveChanges = true)
        {
            var existing = _translationRepository.SingleOrDefault(
                t => t.NameHistoryId == item.NameHistoryId && t.LanguageId == item.LanguageId)
                ?? throw new KeyNotFoundException(
                    $"NameHistoryTranslation with key ({item.NameHistoryId}, {item.LanguageId}) not found.");

            existing.Name = item.Name;
            existing.ShortName = item.ShortName;

            base.Update(existing, saveChanges);
        }
    }
}
