using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class RoleAccessService : BaseService<RoleAccess, RoleAccessDto, RoleAccessDto, RoleAccessDto>, IRoleAccessService
    {
        private readonly IRoleAccessRepository _repository;
        private readonly IAccessService _accessService;

        public RoleAccessService(
            IMapper mapper,
            IRoleAccessRepository repository,
            IAccessService accessService) : base(mapper, repository)
        {
            _repository = repository;
            _accessService = accessService;
        }

        public async Task<List<RoleAccessGrantSummaryDto>> ListGrantsByCompanyAsync(Guid companyId)
        {
            // Per-company rows for this company + all global rows (CompanyId == NULL).
            var rows = await _repository.ToListAsync(
                ra => ra.CompanyId == companyId || ra.CompanyId == null);

            return GroupRows(rows);
        }

        public async Task<List<RoleAccessGrantSummaryDto>> ListAllGrantsAsync()
        {
            var rows = await _repository.ToListAsync();
            return GroupRows(rows);
        }

        public async Task UpsertGrantAsync(RoleAccessGrantDto dto, Guid callerPersonId)
        {
            if (dto.GrantedToRoleId == Guid.Empty)
                throw new BusinessRuleException("نقش هدف الزامی است");

            if (!IsValidLevel(dto.InformationLevel) || !IsValidLevel(dto.AccessLevel))
                throw new BusinessRuleException("سطح دسترسی نامعتبر است");

            // Global RoleAccess rows (CompanyId IS NULL) define system-wide admin
            // visibility. They are infrastructure — managed only via versioned
            // migrations, never via the API. Upserts that delete-then-insert can
            // silently destroy these rows (e.g. saving with both levels = 0).
            if (!dto.CompanyId.HasValue)
                throw new BusinessRuleException("GLOBAL_ROLE_GRANT_MODIFY_FORBIDDEN");

            // Company-scoped grants need Edit on the Access section of that company.
            if (dto.CompanyId.HasValue)
            {
                var levels = await _accessService.GetLevelsAsync(dto.CompanyId.Value, callerPersonId);
                if (levels.Access < 2)
                    throw new BusinessRuleException("شما اجازه اعطای دسترسی نقشی برای این شرکت را ندارید");
            }

            var existing = await _repository.ToListAsync(
                ra => ra.CompanyId == dto.CompanyId && ra.GrantedToRoleId == dto.GrantedToRoleId);

            foreach (var row in existing)
                _repository.Remove(row, false);

            void AddIfPositive(byte sectionId, int level)
            {
                if (level <= 0) return;
                var entity = new RoleAccess
                {
                    Id = Guid.CreateVersion7(),
                    CompanyId = dto.CompanyId,
                    GrantedToRoleId = dto.GrantedToRoleId,
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

        public async Task RevokeByPairAsync(Guid? companyId, Guid grantedToRoleId, Guid callerPersonId)
        {
            // Global RoleAccess rows are immutable infrastructure — protected
            // from accidental deletion. Manage via migrations only.
            if (companyId is null)
                throw new BusinessRuleException("GLOBAL_ROLE_GRANT_DELETE_FORBIDDEN");

            if (companyId.HasValue)
            {
                var levels = await _accessService.GetLevelsAsync(companyId.Value, callerPersonId);
                if (levels.Access < 2)
                    throw new BusinessRuleException("شما اجازه حذف دسترسی نقشی این شرکت را ندارید");
            }

            var rows = await _repository.ToListAsync(
                ra => ra.CompanyId == companyId && ra.GrantedToRoleId == grantedToRoleId);

            if (!rows.Any())
                throw new BusinessRuleException("دسترسی یافت نشد");

            foreach (var row in rows)
                _repository.Remove(row, false);

            await _repository.SaveChangesAsync();
        }

        private static List<RoleAccessGrantSummaryDto> GroupRows(IEnumerable<RoleAccess> rows)
        {
            return rows
                .GroupBy(r => new { r.CompanyId, r.GrantedToRoleId })
                .Select(g =>
                {
                    var groupList = g.ToList();
                    var summary = new RoleAccessGrantSummaryDto
                    {
                        CompanyId = g.Key.CompanyId,
                        GrantedToRoleId = g.Key.GrantedToRoleId,
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
