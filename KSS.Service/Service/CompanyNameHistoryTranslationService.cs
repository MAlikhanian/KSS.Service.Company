using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    /// <summary>
    /// Single-table CRUD service for CompanyNameHistoryTranslation.
    /// Multi-table orchestration (sync to CompanyTranslation, etc.) is handled by CompanyNameManagementService.
    /// </summary>
    public class CompanyNameHistoryTranslationService : BaseService<CompanyNameHistoryTranslation, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto, CompanyNameHistoryTranslationDto>, ICompanyNameHistoryTranslationService
    {
        private readonly ICompanyNameHistoryTranslationRepository _translationRepository;

        public CompanyNameHistoryTranslationService(
            IMapper mapper,
            ICompanyNameHistoryTranslationRepository repository) : base(mapper, repository)
        {
            _translationRepository = repository;
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// Prevents DbUpdateConcurrencyException from _dbSet.Update() on detached entity.
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
