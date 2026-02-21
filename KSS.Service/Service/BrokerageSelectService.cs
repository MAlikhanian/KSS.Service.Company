using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class BrokerageSelectService : IBrokerageSelectService
    {
        private readonly MainDbContext _dbContext;

        public BrokerageSelectService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a lightweight list of companies (brokerages) with their translated name
        /// for use in select dropdowns. Joins Company + CompanyTranslation in a single query.
        /// </summary>
        /// <param name="languageId">Language ID for the translated name (e.g. 12 = Persian, 10 = English)</param>
        /// <param name="query">Optional search query to filter by name (case-insensitive contains)</param>
        public async Task<IEnumerable<BrokerageSelectDto>> GetBrokerageSelectListAsync(short languageId, string? query = null)
        {
            var dbQuery = from c in _dbContext.Companies
                          join ct in _dbContext.CompanyTranslations
                              on new { CompanyId = c.Id, LanguageId = languageId }
                              equals new { ct.CompanyId, ct.LanguageId }
                              into translations
                          from ct in translations.DefaultIfEmpty()
                          select new BrokerageSelectDto
                          {
                              Id = c.Id,
                              Name = ct != null ? ct.Name : c.NationalId,
                              Code = c.RegistrationNo,
                              IsActive = c.IsActive,
                              NationalId = c.NationalId,
                              Website = c.Website
                          };

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(query))
            {
                dbQuery = dbQuery.Where(x => x.Name.Contains(query));
            }

            // Order by name
            dbQuery = dbQuery.OrderBy(x => x.Name);

            return await dbQuery.AsNoTracking().ToListAsync();
        }
    }
}
