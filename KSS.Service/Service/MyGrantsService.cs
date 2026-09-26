using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class MyGrantsService : IMyGrantsService
    {
        private readonly IAccessRepository _accessRepository;
        private readonly IRoleAccessRepository _roleAccessRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ITranslationRepository _translationRepository;

        public MyGrantsService(
            IAccessRepository accessRepository,
            IRoleAccessRepository roleAccessRepository,
            ICompanyRepository companyRepository,
            ITranslationRepository translationRepository)
        {
            _accessRepository = accessRepository;
            _roleAccessRepository = roleAccessRepository;
            _companyRepository = companyRepository;
            _translationRepository = translationRepository;
        }

        public async Task<MyGrantsDto> GetAsync(Guid callerPersonId, IReadOnlyCollection<Guid> callerRoleIds)
        {
            // Fails closed: without a caller there is nobody whose grants these could be.
            if (callerPersonId == Guid.Empty)
                throw new BusinessRuleException("Caller PersonId not found on the JWT.");

            // Every row, live or not: IsActive and IsDeleted travel with each row.
            var personal = await _accessRepository.ToListAsync(a => a.GrantedToPersonId == callerPersonId);
            var roleIds = callerRoleIds.Where(id => id != Guid.Empty).Distinct().ToList();
            var byRole = roleIds.Count == 0
                ? new List<RoleAccess>()
                : (await _roleAccessRepository.ToListAsync(ra => roleIds.Contains(ra.GrantedToRoleId))).ToList();

            var grants = personal
                .Select(a => new MyGrantDto { CompanyId = a.CompanyId, SectionId = a.SectionId, Level = a.Level, IsActive = a.IsActive, IsDeleted = a.DeletedAt != null })
                .Concat(byRole.Select(ra => new MyGrantDto { CompanyId = ra.CompanyId, SectionId = ra.SectionId, Level = ra.Level, IsActive = ra.IsActive, IsDeleted = ra.DeletedAt != null }))
                .OrderBy(g => g.CompanyId.HasValue)
                .ThenBy(g => g.CompanyId)
                .ThenBy(g => g.SectionId)
                .ThenBy(g => g.Level)
                .ToList();

            var result = new MyGrantsDto { Grants = grants };

            // A grant without a company covers every company. It names no company and is not
            // expanded into a list of companies: only the companies named by a grant are returned.
            var companyIds = grants.Where(g => g.CompanyId.HasValue).Select(g => g.CompanyId!.Value).Distinct().ToList();
            if (companyIds.Count == 0)
                return result;

            var companies = await _companyRepository.ToListAsync(c => companyIds.Contains(c.Id));
            // The company's names: every translation that is not deleted.
            var translations = (await _translationRepository.ToListAsync(t => companyIds.Contains(t.CompanyId) && t.DeletedAt == null))
                .ToLookup(t => t.CompanyId);

            result.Companies = companies
                .OrderBy(c => c.Id)
                .Select(c => new MyGrantCompanyDto
                {
                    CompanyId = c.Id,
                    IsActive = c.IsActive,
                    IsDeleted = c.DeletedAt != null,
                    Names = translations[c.Id]
                        .OrderBy(t => t.LanguageId)
                        .Select(t => new CompanyNameDto { LanguageId = t.LanguageId, Name = t.Name, ShortName = t.ShortName })
                        .ToList(),
                })
                .ToList();

            return result;
        }
    }
}
