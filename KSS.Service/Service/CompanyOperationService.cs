using System.Transactions;
using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyOperationService : ICompanyOperationService
    {
        private readonly ICompanyService _companyService;
        private readonly ICompanyTranslationService _translationService;
        private readonly ICompanyNameHistoryService _nameHistoryService;
        private readonly ICompanyNameHistoryTranslationService _nameHistoryTranslationService;

        public CompanyOperationService(
            ICompanyService companyService,
            ICompanyTranslationService translationService,
            ICompanyNameHistoryService nameHistoryService,
            ICompanyNameHistoryTranslationService nameHistoryTranslationService)
        {
            _companyService = companyService;
            _translationService = translationService;
            _nameHistoryService = nameHistoryService;
            _nameHistoryTranslationService = nameHistoryTranslationService;
        }

        public async Task<CompanyDto> CreateCompanyWithTranslationsAndNameHistoryAsync(CompanyInsertDto dto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // 1. Add Company (own table only)
                var companyDto = new CompanyDto
                {
                    Id = dto.Id,
                    CompanyTypeId = dto.CompanyTypeId,
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
                    Website = dto.Website,
                    LogoUrl = dto.LogoUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _companyService.AddDtoAsync(companyDto);

                // 2. Add Company Translations (own table only)
                if (dto.Translations != null && dto.Translations.Any())
                {
                    foreach (var t in dto.Translations)
                    {
                        await _translationService.AddDtoAsync(new CompanyTranslationDto
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

                    var nameHistoryId = Guid.NewGuid();
                    var nameHistoryDto = new CompanyNameHistoryDto
                    {
                        Id = nameHistoryId,
                        CompanyId = companyDto.Id,
                        StartDate = dto.NameHistory.StartDate,
                        EndDate = dto.NameHistory.EndDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _nameHistoryService.AddDtoAsync(nameHistoryDto);

                    // 4. Add Name History Translations (own table only)
                    if (dto.NameHistory.Translations != null && dto.NameHistory.Translations.Any())
                    {
                        foreach (var t in dto.NameHistory.Translations)
                        {
                            await _nameHistoryTranslationService.AddDtoAsync(new CompanyNameHistoryTranslationDto
                            {
                                CompanyNameHistoryId = nameHistoryId,
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
