using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanySelectService : ICompanySelectService
    {
        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public CompanySelectService(MainDbContext dbContext, IAccessService accessService, IHttpContextAccessor? httpContextAccessor = null)
        {
            _dbContext = dbContext;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid? GetCallerPersonId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;
            var raw = user.FindFirstValue("personId") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        /// <summary>
        /// Returns a lightweight list of companies with their translated name
        /// and all name history entries for use in select dropdowns.
        /// </summary>
        /// <param name="languageId">Language ID for the translated name (e.g. 12 = Persian, 10 = English)</param>
        /// <param name="query">Optional search query to filter by current name or any historical name</param>
        public async Task<IEnumerable<CompanySelectDto>> GetCompanySelectListAsync(
            short languageId,
            string? query = null,
            IReadOnlyCollection<Guid>? companyIds = null)
        {
            // Caller passed an explicit ID set that's already empty → nothing to return.
            if (companyIds != null && companyIds.Count == 0)
                return new List<CompanySelectDto>();

            // Visibility: the companies the caller may read, by the same rule as every other
            // read of company information (Information level 1 or more on a live grant; a live
            // global role grant at that level covers every company). A grant on the Access
            // section alone does not make a company visible here, so the list never offers a
            // company whose information the caller cannot open.
            var callerId = GetCallerPersonId();
            if (callerId == null) return new List<CompanySelectDto>();

            var readable = await _accessService.ReadableCompanyIdsAsync(callerId.Value);
            if (!readable.All && readable.CompanyIds.Count == 0) return new List<CompanySelectDto>();

            // Get companies with current name. Apply the visibility filter only
            // when the caller does not read every company, to keep the EF expression
            // tree simple.
            var allowedList = readable.All ? null : readable.CompanyIds.ToList();
            IQueryable<KSS.Entity.Company> visibleCompanies = _dbContext.Companies;
            if (!readable.All)
                visibleCompanies = visibleCompanies.Where(c => allowedList!.Contains(c.Id));

            // Optional explicit-ID filter, applied AFTER the access filter so it can
            // only narrow visibility — never widen it.
            if (companyIds != null)
            {
                var idList = companyIds.ToList();
                visibleCompanies = visibleCompanies.Where(c => idList.Contains(c.Id));
            }

            var companyRows = await visibleCompanies
                .Select(c => new { c.Id, Code = c.RegistrationNo, c.IsActive, c.NationalId })
                .AsNoTracking()
                .ToListAsync();

            // Translations in any language for the visible companies; the name is chosen in
            // memory (requested language first, then any other; the registry id only when
            // no language has a name).
            var translationsByCompany = (await (from t in _dbContext.Translations
                                                join c in visibleCompanies on t.CompanyId equals c.Id
                                                select new { t.CompanyId, t.LanguageId, t.Name })
                                                .AsNoTracking()
                                                .ToListAsync())
                .ToLookup(t => t.CompanyId);

            var companies = companyRows.Select(c => new
            {
                c.Id,
                Name = CompanyDisplayName.PickCompanyDisplayName(
                    translationsByCompany[c.Id].Select(t => (t.LanguageId, (string?)t.Name)),
                    languageId,
                    c.NationalId),
                c.Code,
                c.IsActive,
                c.NationalId
            }).ToList();

            // Get all name history with translations for the requested language
            var nameHistories = await (from h in _dbContext.NameHistories
                                       join ht in _dbContext.NameHistoryTranslations
                                           on new { HistoryId = h.Id, LanguageId = languageId }
                                           equals new { HistoryId = ht.NameHistoryId, ht.LanguageId }
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
                    NationalId = c.NationalId
                };

                if (historyByCompany.TryGetValue(c.Id, out var histories))
                {
                    dto.NameHistory = histories.Select(h => new NameHistoryDto
                    {
                        Id = h.Id,
                        Name = h.Name,
                        StartDate = h.StartDate,
                        EndDate = h.EndDate
                    }).ToList();
                }

                return dto;
            });

            // Filter by search query (match current name, historical name, or nationalId)
            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(x =>
                    x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (x.NationalId != null && x.NationalId.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    x.NameHistory.Any(h => h.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            return result.OrderBy(x => x.Name).ToList();
        }
    }
}
