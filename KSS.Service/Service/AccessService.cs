using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Service
{
    public class AccessService : BaseService<Access, AccessDto, AccessAddDto, AccessDto>, IAccessService
    {
        private readonly IAccessRepository _repository;
        private readonly IRoleAccessRepository _roleAccessRepository;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AccessService(
            IMapper mapper,
            IAccessRepository repository,
            IRoleAccessRepository roleAccessRepository,
            IHttpContextAccessor? httpContextAccessor = null) : base(mapper, repository)
        {
            _repository = repository;
            _roleAccessRepository = roleAccessRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        // Reads "roleId" claims from the JWT — used to evaluate RoleAccess rows.
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

        // Reads the "personId" claim from the JWT — the calling user's person id.
        private Guid GetCallerPersonId()
        {
            var raw = _httpContextAccessor?.HttpContext?.User?.FindFirst("personId")?.Value;
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }

        public async Task SeedCreatorAccessAsync(Guid companyId)
        {
            var creatorId = GetCallerPersonId();
            if (creatorId == Guid.Empty) return; // system/seed context — no creator to grant.

            // Grant the creator Edit on the Information section only. The Access
            // section is deliberately omitted, so the creator can edit the
            // company's data but cannot manage its access list. This is the
            // initial-owner seed, so it does NOT run the Access-edit gate that
            // UpsertGrantAsync enforces for later grants.
            var entity = new Access
            {
                Id = Guid.CreateVersion7(),
                CompanyId = companyId,
                GrantedToPersonId = creatorId,
                SectionId = AccessSectionId.Information,
                Level = 2,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
            _repository.AddUnawaited(entity, false);
            await _repository.SaveChangesAsync();
        }

        public async Task<AccessLevelsDto> GetLevelsAsync(Guid companyId, Guid callerPersonId)
        {
            // Note: there's no Company.CreatedBy column today, so no owner
            // short-circuit. Admins get full access via the seeded RoleAccess
            // globals (CompanyId IS NULL) for SuperAdmin and CompanyAdmin.

            // Only live grants count: an inactive or deleted row grants nothing.
            var rows = await _repository.ToListAsync(
                a => a.CompanyId == companyId && a.GrantedToPersonId == callerPersonId
                  && a.IsActive && a.DeletedAt == null);

            var dto = new AccessLevelsDto();
            foreach (var row in rows)
            {
                switch (row.SectionId)
                {
                    case AccessSectionId.Information: dto.Information = row.Level; break;
                    case AccessSectionId.Access:      dto.Access      = row.Level; break;
                }
            }

            // Role overlay: RoleAccess rows matching the caller's roleId claims,
            // either global (CompanyId IS NULL) or scoped to this company.
            // Role grants raise the level — never lower it.
            var roleIds = GetCallerRoleIds();
            if (roleIds.Count > 0)
            {
                var roleRows = await _roleAccessRepository.ToListAsync(
                    ra => roleIds.Contains(ra.GrantedToRoleId)
                       && (ra.CompanyId == companyId || ra.CompanyId == null)
                       && ra.IsActive && ra.DeletedAt == null);

                foreach (var row in roleRows)
                {
                    switch (row.SectionId)
                    {
                        case AccessSectionId.Information: dto.Information = Math.Max(dto.Information, row.Level); break;
                        case AccessSectionId.Access:      dto.Access      = Math.Max(dto.Access,      row.Level); break;
                    }
                }
            }

            return dto;
        }

        // The read rule for lists, matching GetLevelsAsync company by company: Information
        // level 1 or more on a live grant, personal or through one of the caller's roles.
        // A live global role grant at that level covers every company; a global grant on the
        // Access section alone does not. Fails closed: no caller reads nothing.
        public async Task<ReadableCompanies> ReadableCompanyIdsAsync(Guid callerPersonId)
        {
            if (callerPersonId == Guid.Empty)
                return ReadableCompanies.None;

            var roleIds = GetCallerRoleIds();
            var roleRows = roleIds.Count == 0
                ? new List<RoleAccess>()
                : (await _roleAccessRepository.ToListAsync(
                    ra => roleIds.Contains(ra.GrantedToRoleId)
                       && ra.SectionId == AccessSectionId.Information && ra.Level >= ReadLevel
                       && ra.IsActive && ra.DeletedAt == null)).ToList();

            if (roleRows.Any(ra => ra.CompanyId == null))
                return ReadableCompanies.Every;

            var personal = await _repository.ToListAsync(
                a => a.GrantedToPersonId == callerPersonId
                  && a.SectionId == AccessSectionId.Information && a.Level >= ReadLevel
                  && a.IsActive && a.DeletedAt == null);

            var companyIds = new HashSet<Guid>(personal.Select(a => a.CompanyId));
            companyIds.UnionWith(roleRows.Select(ra => ra.CompanyId!.Value));
            return new ReadableCompanies(false, companyIds);
        }

        public async Task UpsertGrantAsync(AccessGrantDto dto, Guid callerPersonId)
        {
            if (dto.GrantedToPersonId == Guid.Empty)
                throw new BusinessRuleException("شخص هدف الزامی است");

            if (!IsValidLevel(dto.InformationLevel) || !IsValidLevel(dto.AccessLevel))
                throw new BusinessRuleException("سطح دسترسی نامعتبر است");

            // Authorization: caller must have Edit (level 2) on the Access section
            // of this specific company. Admins get this via global RoleAccess.
            var levels = await GetLevelsAsync(dto.CompanyId, callerPersonId);
            if (levels.Access < 2)
                throw new BusinessRuleException("شما اجازه اعطای دسترسی برای این شرکت را ندارید");

            // Replace all rows for this (CompanyId, GrantedToPersonId) pair, inactive and
            // deleted ones included: UQ_Access (CompanyId, GrantedToPersonId, SectionId) does
            // not include IsActive, so a row left behind would block the insert below.
            var existing = await _repository.ToListAsync(
                a => a.CompanyId == dto.CompanyId && a.GrantedToPersonId == dto.GrantedToPersonId);

            foreach (var row in existing)
                _repository.Remove(row, false);

            void AddIfPositive(byte sectionId, int level)
            {
                if (level <= 0) return;
                var entity = new Access
                {
                    Id = Guid.CreateVersion7(),
                    CompanyId = dto.CompanyId,
                    GrantedToPersonId = dto.GrantedToPersonId,
                    SectionId = sectionId,
                    Level = level,
                    CreatedBy = callerPersonId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };
                _repository.AddUnawaited(entity, false);
            }

            AddIfPositive(AccessSectionId.Information, dto.InformationLevel);
            AddIfPositive(AccessSectionId.Access,      dto.AccessLevel);

            await _repository.SaveChangesAsync();
        }

        public async Task RevokeByPairAsync(Guid companyId, Guid grantedToPersonId, Guid callerPersonId)
        {
            var levels = await GetLevelsAsync(companyId, callerPersonId);
            if (levels.Access < 2)
                throw new BusinessRuleException("شما اجازه حذف دسترسی این شرکت را ندارید");

            // Every row of the pair, inactive and deleted ones included, so a revocation
            // leaves nothing behind.
            var rows = await _repository.ToListAsync(
                a => a.CompanyId == companyId && a.GrantedToPersonId == grantedToPersonId);

            if (!rows.Any())
                throw new BusinessRuleException("دسترسی یافت نشد");

            foreach (var row in rows)
                _repository.Remove(row, false);

            await _repository.SaveChangesAsync();
        }

        public async Task<List<AccessGrantSummaryDto>> ListGrantsByCompanyAsync(Guid companyId)
        {
            var rows = await _repository.ToListAsync(
                a => a.CompanyId == companyId && a.IsActive && a.DeletedAt == null);

            return rows
                .GroupBy(r => r.GrantedToPersonId)
                .Select(g =>
                {
                    var groupList = g.ToList();
                    var summary = new AccessGrantSummaryDto
                    {
                        CompanyId = companyId,
                        GrantedToPersonId = g.Key,
                        CreatedAt = groupList.Min(r => r.CreatedAt),
                        UpdatedAt = groupList.Max(r => r.UpdatedAt),
                    };
                    foreach (var row in groupList)
                    {
                        switch (row.SectionId)
                        {
                            case AccessSectionId.Information: summary.InformationLevel = row.Level; break;
                            case AccessSectionId.Access:      summary.AccessLevel      = row.Level; break;
                        }
                    }
                    return summary;
                })
                .ToList();
        }

        public async Task<List<AccessGrantPairDto>> ListAllGrantPairsAsync()
        {
            // Walk every row in the Access table; collapse multi-section rows
            // into a distinct (CompanyId, GrantedToPersonId) pair so consumers
            // get exactly one row per (company, grantee).
            var rows = await _repository.ToListAsync(a => a.IsActive && a.DeletedAt == null);
            return rows
                .GroupBy(r => new { r.CompanyId, r.GrantedToPersonId })
                .Select(g => new AccessGrantPairDto
                {
                    CompanyId = g.Key.CompanyId,
                    GrantedToPersonId = g.Key.GrantedToPersonId,
                })
                .ToList();
        }

        private const int ReadLevel = 1;

        private static bool IsValidLevel(int level) => level >= 0 && level <= 2;
    }
}
