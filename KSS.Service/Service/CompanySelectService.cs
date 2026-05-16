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
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public CompanySelectService(MainDbContext dbContext, IHttpContextAccessor? httpContextAccessor = null)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid? GetCallerPersonId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;
            var raw = user.FindFirstValue("personId") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        private List<Guid> GetCallerRoleIds()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return new List<Guid>();
            return user.FindAll("roleId")
                .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Returns a lightweight list of companies with their translated name
        /// and all name history entries for use in select dropdowns.
        /// </summary>
        /// <param name="languageId">Language ID for the translated name (e.g. 12 = Persian, 10 = English)</param>
        /// <param name="query">Optional search query to filter by current name or any historical name</param>
        public async Task<IEnumerable<CompanySelectDto>> GetCompanySelectListAsync(short languageId, string? query = null)
        {
            // Visibility filter: caller must have an Access row (per-person) OR a
            // RoleAccess row (per-company or global) for the company to be returned.
            var callerId = GetCallerPersonId();
            var roleIds = GetCallerRoleIds();

            if (callerId == null) return new List<CompanySelectDto>();

            // Global short-circuit: any RoleAccess row with CompanyId IS NULL whose
            // role matches the caller → caller sees every company (admins).
            var hasGlobal = roleIds.Count > 0 && await _dbContext.RoleAccesses
                .AsNoTracking()
                .AnyAsync(ra => ra.CompanyId == null && roleIds.Contains(ra.GrantedToRoleId));

            HashSet<Guid>? allowedCompanyIds = null;
            if (!hasGlobal)
            {
                var personalIds = await _dbContext.Accesses
                    .AsNoTracking()
                    .Where(a => a.GrantedToPersonId == callerId.Value)
                    .Select(a => a.CompanyId)
                    .Distinct()
                    .ToListAsync();

                var roleScopedIds = roleIds.Count == 0
                    ? new List<Guid>()
                    : await _dbContext.RoleAccesses
                        .AsNoTracking()
                        .Where(ra => ra.CompanyId != null && roleIds.Contains(ra.GrantedToRoleId))
                        .Select(ra => ra.CompanyId!.Value)
                        .Distinct()
                        .ToListAsync();

                allowedCompanyIds = new HashSet<Guid>(personalIds.Concat(roleScopedIds));
                if (allowedCompanyIds.Count == 0) return new List<CompanySelectDto>();
            }

            // Get companies with current name. Apply the visibility filter only
            // when there is no global short-circuit, to keep the EF expression
            // tree simple.
            var allowedList = allowedCompanyIds?.ToList();
            IQueryable<KSS.Entity.Company> visibleCompanies = _dbContext.Companies;
            if (!hasGlobal)
                visibleCompanies = visibleCompanies.Where(c => allowedList!.Contains(c.Id));

            var companiesQuery = from c in visibleCompanies
                                 join ct in _dbContext.Translations
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
                    NationalId = c.NationalId,
                    Website = c.Website
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
