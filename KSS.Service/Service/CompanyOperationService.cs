using System.Transactions;
using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyOperationService : ICompanyOperationService
    {
        private readonly ICompanyService _companyService;
        private readonly ITranslationService _translationService;
        private readonly INameHistoryService _nameHistoryService;
        private readonly INameHistoryTranslationService _nameHistoryTranslationService;
        private readonly IAccessService _accessService;
        private readonly IMapper _mapper;

        public CompanyOperationService(
            ICompanyService companyService,
            ITranslationService translationService,
            INameHistoryService nameHistoryService,
            INameHistoryTranslationService nameHistoryTranslationService,
            IAccessService accessService,
            IMapper mapper)
        {
            _companyService = companyService;
            _translationService = translationService;
            _nameHistoryService = nameHistoryService;
            _nameHistoryTranslationService = nameHistoryTranslationService;
            _accessService = accessService;
            _mapper = mapper;
        }

        public async Task<CompanyDto> CreateCompanyWithTranslationsAndNameHistoryAsync(CompanyInsertDto dto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // 1. Add Company (own table only). The id is generated here (v7)
                // because the translations + name-history below need it as a FK
                // within this same transaction. The frontend never supplies it.
                var companyDto = new CompanyDto
                {
                    Id = Guid.CreateVersion7(),
                    LegalFormId = dto.LegalFormId,
                    IndustryId = dto.IndustryId,
                    RegistrationDate = dto.RegistrationDate,
                    RegistrationNo = dto.RegistrationNo,
                    NationalId = dto.NationalId,
                    EconomicCode = dto.EconomicCode,
                    RegistrationCountryId = dto.RegistrationCountryId,
                    RegistrationRegionId = dto.RegistrationRegionId,
                    RegistrationCityId = dto.RegistrationCityId,
                    TaxId = dto.TaxId,
                    FoundedDate = dto.FoundedDate,
                    LogoUrl = dto.LogoUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                // Insert with the explicitly-generated v7 id preserved (the child rows
                // below FK to it). Entity-level AddAsync keeps the id; the public
                // AddDto endpoint uses CompanyInsertDto (no Id) for external callers.
                await _companyService.AddAsync(_mapper.Map<Company>(companyDto));

                // 1b. Grant the creator row-level access on the new company:
                // Information = Edit, Access section omitted (creator can edit the
                // company but not manage its access list). Same transaction.
                await _accessService.SeedCreatorAccessAsync(companyDto.Id);

                // 2. Add Company Translations (own table only)
                if (dto.Translations != null && dto.Translations.Any())
                {
                    // The company id is server-generated here; the per-company guard adds no
                    // protection and would depend on uncommitted-row visibility.
                    var newCompanyTranslation = (INewCompanyTranslation)_translationService;
                    foreach (var t in dto.Translations)
                    {
                        await newCompanyTranslation.AddForNewCompanyAsync(new TranslationDto
                        {
                            CompanyId = companyDto.Id,
                            LanguageId = t.LanguageId,
                            Name = t.Name,
                            ShortName = t.ShortName,
                            Description = t.Description
                        });
                    }
                }

                // 3. Add Name History if provided (own table only)
                if (dto.NameHistory != null)
                {
                    if (dto.NameHistory.EndDate.HasValue && dto.NameHistory.StartDate > dto.NameHistory.EndDate.Value)
                    {
                        throw new ArgumentException("StartDate must be less than or equal to EndDate.", nameof(dto));
                    }

                    var nameHistoryId = Guid.CreateVersion7();
                    var nameHistoryDto = new NameHistoryDto
                    {
                        Id = nameHistoryId,
                        CompanyId = companyDto.Id,
                        StartDate = dto.NameHistory.StartDate,
                        EndDate = dto.NameHistory.EndDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    // The company id is server-generated here; the per-company guard adds no
                    // protection and would depend on uncommitted-row visibility.
                    var newCompanyNameHistory = (INewCompanyNameHistory)_nameHistoryService;
                    await newCompanyNameHistory.AddForNewCompanyAsync(_mapper.Map<NameHistory>(nameHistoryDto));

                    // 4. Add Name History Translations (own table only)
                    if (dto.NameHistory.Translations != null && dto.NameHistory.Translations.Any())
                    {
                        // Same reason as above: the entry belongs to the company created here.
                        var newCompanyNameHistoryTranslation = (INewCompanyNameHistoryTranslation)_nameHistoryTranslationService;
                        foreach (var t in dto.NameHistory.Translations)
                        {
                            await newCompanyNameHistoryTranslation.AddForNewCompanyAsync(new NameHistoryTranslationDto
                            {
                                NameHistoryId = nameHistoryId,
                                LanguageId = t.LanguageId,
                                Name = t.Name,
                                ShortName = t.ShortName
                            });
                        }
                    }
                }

                transactionScope.Complete();
                return companyDto;
            }
            catch
            {
                throw;
            }
        }
    }
}
