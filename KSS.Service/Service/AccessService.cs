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

        public async Task<AccessLevelsDto> GetLevelsAsync(Guid companyId, Guid callerPersonId)
        {
            // Note: there's no Company.CreatedBy column today, so no owner
            // short-circuit. Admins get full access via the seeded RoleAccess
            // globals (CompanyId IS NULL) for SuperAdmin and CompanyAdmin.

            var rows = await _repository.ToListAsync(
                a => a.CompanyId == companyId && a.GrantedToPersonId == callerPersonId);

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
                       && (ra.CompanyId == companyId || ra.CompanyId == null));

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

            // Replace all rows for this (CompanyId, GrantedToPersonId) pair.
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
            var rows = await _repository.ToListAsync(a => a.CompanyId == companyId);

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

        private static bool IsValidLevel(int level) => level >= 0 && level <= 2;
    }
}
