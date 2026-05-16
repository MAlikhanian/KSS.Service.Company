using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    /// <summary>
    /// ManagementService that builds the consolidated read-only company view.
    /// Pulls from Company, Translation, NameHistory, NameHistoryTranslation,
    /// Email, EmailLabelTranslation, Phone, PhoneLabelTranslation, Address,
    /// AddressLabelTranslation, AddressTranslation in a single request.
    ///
    /// Country/Region/City names are NOT resolved here — those lookups live
    /// in KSS.Service.Common. The BFF layer resolves IDs to names before
    /// returning to the frontend, keeping this service strictly inside the
    /// Company domain.
    /// </summary>
    public class CompanyReadViewManagementService : ICompanyReadViewManagementService
    {
        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;

        public CompanyReadViewManagementService(MainDbContext dbContext, IAccessService accessService)
        {
            _dbContext = dbContext;
            _accessService = accessService;
        }

        public async Task<CompanyReadViewDto?> GetByIdAsync(Guid companyId, Guid callerPersonId, short languageId = 12)
        {
            // Row-level access gate. Caller must have Information.Read (>=1) on
            // THIS company. Null → controller responds 404, no existence leak.
            var levels = await _accessService.GetLevelsAsync(companyId, callerPersonId);
            if (levels.Information < 1) return null;

            var company = await _dbContext.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null) return null;

            // Persian (12) + English (10) translations for the current company name.
            var translations = await _dbContext.Translations
                .AsNoTracking()
                .Where(t => t.CompanyId == companyId && (t.LanguageId == 12 || t.LanguageId == 10))
                .ToListAsync();
            var persianName = translations.FirstOrDefault(t => t.LanguageId == 12)?.Name ?? string.Empty;
            var latinName = translations.FirstOrDefault(t => t.LanguageId == 10)?.Name;

            // Name history with ALL translations per entry (not filtered by languageId).
            var nameHistoryEntities = await _dbContext.NameHistories
                .AsNoTracking()
                .Where(h => h.CompanyId == companyId)
                .OrderByDescending(h => h.StartDate)
                .ToListAsync();

            var historyIds = nameHistoryEntities.Select(h => h.Id).ToList();
            var nameHistoryTranslations = await _dbContext.NameHistoryTranslations
                .AsNoTracking()
                .Where(t => historyIds.Contains(t.NameHistoryId))
                .ToListAsync();
            var translationsByHistory = nameHistoryTranslations
                .GroupBy(t => t.NameHistoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var nameHistory = nameHistoryEntities.Select(h => new CompanyReadViewNameHistoryDto
            {
                Id = h.Id,
                StartDate = h.StartDate,
                EndDate = h.EndDate,
                Description = h.Description,
                Translations = translationsByHistory.TryGetValue(h.Id, out var trs)
                    ? trs.Select(t => new CompanyReadViewNameHistoryTranslationDto
                    {
                        LanguageId = t.LanguageId,
                        Name = t.Name,
                    }).ToList()
                    : new List<CompanyReadViewNameHistoryTranslationDto>(),
            }).ToList();

            // Emails with label name (mirrors CompanyContactService).
            var emails = await (from e in _dbContext.Emails
                                where e.CompanyId == companyId
                                join lt in _dbContext.EmailLabelTranslations
                                    on new { e.LabelId, LanguageId = languageId }
                                    equals new { LabelId = lt.EmailLabelId, lt.LanguageId }
                                    into labelJoin
                                from lt in labelJoin.DefaultIfEmpty()
                                orderby e.IsPrimary descending, e.EmailAddress
                                select new CompanyReadViewEmailDto
                                {
                                    Id = e.Id,
                                    LabelId = e.LabelId,
                                    LabelName = lt != null ? lt.Name : string.Empty,
                                    EmailAddress = e.EmailAddress,
                                    IsPrimary = e.IsPrimary,
                                    IsVerified = e.IsVerified,
                                }).AsNoTracking().ToListAsync();

            // Phones with label name.
            var phones = await (from p in _dbContext.Phones
                                where p.CompanyId == companyId
                                join lt in _dbContext.PhoneLabelTranslations
                                    on new { p.LabelId, LanguageId = languageId }
                                    equals new { LabelId = lt.PhoneLabelId, lt.LanguageId }
                                    into labelJoin
                                from lt in labelJoin.DefaultIfEmpty()
                                orderby p.IsPrimary descending, p.PhoneNumber
                                select new CompanyReadViewPhoneDto
                                {
                                    Id = p.Id,
                                    LabelId = p.LabelId,
                                    LabelName = lt != null ? lt.Name : string.Empty,
                                    CountryId = p.CountryId,
                                    PhoneNumber = p.PhoneNumber,
                                    IsPrimary = p.IsPrimary,
                                    IsVerified = p.IsVerified,
                                }).AsNoTracking().ToListAsync();

            // Addresses with label name + street translation.
            var addresses = await (from a in _dbContext.Addresses
                                   where a.CompanyId == companyId
                                   join lt in _dbContext.AddressLabelTranslations
                                       on new { a.LabelId, LanguageId = languageId }
                                       equals new { LabelId = lt.AddressLabelId, lt.LanguageId }
                                       into labelJoin
                                   from lt in labelJoin.DefaultIfEmpty()
                                   join at in _dbContext.AddressTranslations
                                       on new { AddressId = a.Id, LanguageId = languageId }
                                       equals new { at.AddressId, at.LanguageId }
                                       into transJoin
                                   from at in transJoin.DefaultIfEmpty()
                                   orderby a.IsPrimary descending
                                   select new CompanyReadViewAddressDto
                                   {
                                       Id = a.Id,
                                       LabelId = a.LabelId,
                                       LabelName = lt != null ? lt.Name : string.Empty,
                                       CountryId = a.CountryId,
                                       RegionId = a.RegionId,
                                       CityId = a.CityId,
                                       PostalCode = a.PostalCode,
                                       Street1 = at != null ? at.Street1 : string.Empty,
                                       Street2 = at != null ? at.Street2 : null,
                                       IsPrimary = a.IsPrimary,
                                       IsVerified = a.IsVerified,
                                   }).AsNoTracking().ToListAsync();

            return new CompanyReadViewDto
            {
                Id = company.Id,
                CompanyPersianName = persianName,
                CompanyLatinName = latinName,
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
                NameHistory = nameHistory,
                Emails = emails,
                Phones = phones,
                Addresses = addresses,
            };
        }
    }
}
