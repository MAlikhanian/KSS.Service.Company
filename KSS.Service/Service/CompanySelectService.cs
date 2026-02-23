using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanySelectService : ICompanySelectService
    {
        private readonly MainDbContext _dbContext;

        public CompanySelectService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a lightweight list of companies with their translated name
        /// and all name history entries for use in select dropdowns.
        /// </summary>
        /// <param name="languageId">Language ID for the translated name (e.g. 12 = Persian, 10 = English)</param>
        /// <param name="query">Optional search query to filter by current name or any historical name</param>
        public async Task<IEnumerable<CompanySelectDto>> GetCompanySelectListAsync(short languageId, string? query = null)
        {
            // Get companies with current name
            var companiesQuery = from c in _dbContext.Companies
                                 join ct in _dbContext.CompanyTranslations
                                     on new { CompanyId = c.Id, LanguageId = languageId }
                                     equals new { ct.CompanyId, ct.LanguageId }
                                     into translations
                                 from ct in translations.DefaultIfEmpty()
                                 select new
                                 {
                                     c.Id,
                                     Name = ct != null ? ct.Name : c.NationalId,
                                     Code = c.RegistrationNo,
                                     c.IsActive,
                                     c.NationalId,
                                     c.Website
                                 };

            var companies = await companiesQuery.AsNoTracking().ToListAsync();

            // Get all name history with translations for the requested language
            var nameHistories = await (from h in _dbContext.CompanyNameHistories
                                       join ht in _dbContext.CompanyNameHistoryTranslations
                                           on new { HistoryId = h.Id, LanguageId = languageId }
                                           equals new { HistoryId = ht.CompanyNameHistoryId, ht.LanguageId }
                                           into htJoin
                                       from ht in htJoin.DefaultIfEmpty()
                                       orderby h.StartDate descending
                                       select new
                                       {
                                           h.CompanyId,
                                           h.Id,
                                           Name = ht != null ? ht.Name : string.Empty,
                                           h.StartDate,
                                           h.EndDate
                                       }).AsNoTracking().ToListAsync();

            // Group name histories by company
            var historyByCompany = nameHistories.GroupBy(h => h.CompanyId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build result
            var result = companies.Select(c =>
            {
                var dto = new CompanySelectDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    IsActive = c.IsActive,
                    NationalId = c.NationalId,
                    Website = c.Website
                };

                if (historyByCompany.TryGetValue(c.Id, out var histories))
                {
                    dto.NameHistory = histories.Select(h => new CompanyNameHistoryDto
                    {
                        Id = h.Id,
                        Name = h.Name,
                        StartDate = h.StartDate,
                        EndDate = h.EndDate
                    }).ToList();
                }

                return dto;
            });

            // Filter by search query (match current name OR any historical name)
            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(x =>
                    x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    x.NameHistory.Any(h => h.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            return result.OrderBy(x => x.Name).ToList();
        }
    }
}
