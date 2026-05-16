using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Helper;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyDetailService : ICompanyDetailService
    {
        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;

        public CompanyDetailService(MainDbContext dbContext, IAccessService accessService)
        {
            _dbContext = dbContext;
            _accessService = accessService;
        }

        public async Task<CompanyDetailDto?> GetByIdAsync(Guid id, Guid callerPersonId, short languageId = 12)
        {
            // Row-level access gate. Caller must have Information.Read (>=1) on
            // THIS company. Returning null lets the controller respond 404 so we
            // do not leak the existence of a company the caller cannot see.
            var levels = await _accessService.GetLevelsAsync(id, callerPersonId);
            if (levels.Information < 1) return null;

            var company = await _dbContext.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return null;

            // Get Persian translation (languageId = 12)
            var persianTranslation = await _dbContext.Translations
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 12);

            // Get English translation (languageId = 10)
            var englishTranslation = await _dbContext.Translations
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.CompanyId == id && ct.LanguageId == 10);

            // Get name history with translations
            var nameHistories = await (from h in _dbContext.NameHistories
                                       where h.CompanyId == id
                                       join ht in _dbContext.NameHistoryTranslations
                                           on new { HistoryId = h.Id, LanguageId = languageId }
                                           equals new { HistoryId = ht.NameHistoryId, ht.LanguageId }
                                           into htJoin
                                       from ht in htJoin.DefaultIfEmpty()
                                       orderby h.StartDate descending
                                       select new NameHistoryDto
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

        public async Task<CompanyDetailDto> UpdateAsync(Guid id, Guid callerPersonId, CompanyDetailDto dto)
        {
            // Row-level access gate. Caller must have Information.Modify (>=2) on
            // THIS company. Bare 'Company.Information.Modify' JWT claim is not
            // enough — the caller also needs a personal Access row or a global
            // RoleAccess row that grants Level 2 here.
            var levels = await _accessService.GetLevelsAsync(id, callerPersonId);
            if (levels.Information < 2)
                throw new BusinessRuleException("You do not have permission to modify this company.");

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

            // NOTE: Company name (Translation) is NOT updated here.
            // Names are managed exclusively through CompanyNameManagementService
            // to keep Translation in sync with NameHistory.

            await _dbContext.SaveChangesAsync();

            // Return fresh data (re-runs the access check, which is fine — caller
            // just succeeded a Modify; Read is implied).
            return (await GetByIdAsync(id, callerPersonId))!;
        }
    }
}
