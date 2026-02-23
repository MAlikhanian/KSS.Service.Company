using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyDetailService : ICompanyDetailService
    {
        private readonly MainDbContext _dbContext;

        public CompanyDetailService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CompanyDetailDto?> GetByIdAsync(Guid id, short languageId = 12)
        {
            var company = await _dbContext.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return null;

            // Get Persian translation (languageId = 12)
            var persianTranslation = await _dbContext.CompanyTranslations
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 12);

            // Get English translation (languageId = 10)
            var englishTranslation = await _dbContext.CompanyTranslations
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 10);

            // Get name history with translations
            var nameHistories = await (from h in _dbContext.CompanyNameHistories
                                       where h.CompanyId == id
                                       join ht in _dbContext.CompanyNameHistoryTranslations
                                           on new { HistoryId = h.Id, LanguageId = languageId }
                                           equals new { HistoryId = ht.CompanyNameHistoryId, ht.LanguageId }
                                           into htJoin
                                       from ht in htJoin.DefaultIfEmpty()
                                       orderby h.StartDate descending
                                       select new CompanyNameHistoryDto
                                       {
                                           Id = h.Id,
                                           Name = ht != null ? ht.Name : string.Empty,
                                           StartDate = h.StartDate,
                                           EndDate = h.EndDate
                                       }).AsNoTracking().ToListAsync();

            // Build former names from past name history entries (where EndDate is not null)
            var pastNames = nameHistories
                .Where(h => h.EndDate != null && !string.IsNullOrEmpty(h.Name))
                .Select(h => h.Name);
            var formerNames = pastNames.Any() ? string.Join("، ", pastNames) : null;

            return new CompanyDetailDto
            {
                Id = company.Id,
                CompanyPersianName = persianTranslation?.Name ?? company.NationalId,
                CompanyLatinName = englishTranslation?.Name,
                FormerNames = formerNames,
                RegistrationDate = company.RegistrationDate,
                RegistrationNo = company.RegistrationNo,
                NationalId = company.NationalId,
                EconomicCode = company.EconomicCode,
                RegistrationCountryId = company.RegistrationCountryId,
                RegistrationRegionId = company.RegistrationRegionId,
                RegistrationCityId = company.RegistrationCityId,
                FoundedDate = company.FoundedDate,
                Website = company.Website,
                IsActive = company.IsActive,
                NameHistory = nameHistories
            };
        }

        public async Task<CompanyDetailDto> UpdateAsync(Guid id, CompanyDetailDto dto)
        {
            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                throw new KeyNotFoundException($"Company with id {id} not found.");

            // Update Company fields
            company.RegistrationDate = dto.RegistrationDate;
            company.RegistrationNo = dto.RegistrationNo;
            company.NationalId = dto.NationalId;
            company.EconomicCode = dto.EconomicCode;
            company.RegistrationCountryId = dto.RegistrationCountryId;
            company.RegistrationRegionId = dto.RegistrationRegionId;
            company.RegistrationCityId = dto.RegistrationCityId;
            company.FoundedDate = dto.FoundedDate;
            company.Website = dto.Website;
            company.IsActive = dto.IsActive;

            // Update Persian translation
            var persianTranslation = await _dbContext.CompanyTranslations
                .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 12);

            if (persianTranslation != null)
            {
                persianTranslation.Name = dto.CompanyPersianName;
            }
            else
            {
                _dbContext.CompanyTranslations.Add(new Entity.CompanyTranslation
                {
                    CompanyId = id,
                    LanguageId = 12,
                    Name = dto.CompanyPersianName
                });
            }

            // Update English translation
            if (!string.IsNullOrWhiteSpace(dto.CompanyLatinName))
            {
                var englishTranslation = await _dbContext.CompanyTranslations
                    .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 10);

                if (englishTranslation != null)
                {
                    englishTranslation.Name = dto.CompanyLatinName;
                }
                else
                {
                    _dbContext.CompanyTranslations.Add(new Entity.CompanyTranslation
                    {
                        CompanyId = id,
                        LanguageId = 10,
                        Name = dto.CompanyLatinName
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            // Return fresh data
            return (await GetByIdAsync(id))!;
        }
    }
}
