using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanySoftwareManagementService : ICompanySoftwareManagementService
    {
        private const int ModifyLevel = 2;
        private const string ModifyDenied = "You do not have permission to modify this company's software.";

        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanySoftwareManagementService(MainDbContext dbContext, IAccessService accessService, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CompanySoftwareSlotDto>?> GetSlotsAsync(Guid companyId, Guid callerPersonId, short languageId = 12)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, callerPersonId);
            if (levels.Information < 1) return null;

            // All active categories, left-joined to this company's pick + software name + translated category name.
            var slots = await (
                from cat in _dbContext.SoftwareCategories
                where cat.IsActive
                join t in _dbContext.SoftwareCategoryTranslations
                    on new { CatId = cat.Id, LanguageId = languageId }
                    equals new { CatId = t.SoftwareCategoryId, t.LanguageId }
                    into tJoin
                from t in tJoin.DefaultIfEmpty()
                join cs in _dbContext.CompanySoftwares.Where(x => x.CompanyId == companyId)
                    on cat.Id equals cs.SoftwareCategoryId into csJoin
                from cs in csJoin.DefaultIfEmpty()
                join sw in _dbContext.Softwares
                    on (cs != null ? cs.SoftwareId : 0) equals sw.Id into swJoin
                from sw in swJoin.DefaultIfEmpty()
                orderby cat.Id
                select new CompanySoftwareSlotDto
                {
                    SoftwareCategoryId = cat.Id,
                    CategoryCode = cat.Code,
                    CategoryName = t != null ? t.Name : cat.Code,
                    SoftwareId = cs != null ? (int?)cs.SoftwareId : null,
                    SoftwareName = sw != null ? sw.Name : null,
                    ProviderCompanyId = sw != null ? (Guid?)sw.CompanyId : null,
                    ProviderCompanyName = sw != null
                        ? _dbContext.Translations
                            .Where(tr => tr.CompanyId == sw.CompanyId && tr.LanguageId == languageId)
                            .Select(tr => tr.Name).FirstOrDefault()
                        : null
                }).AsNoTracking().ToListAsync();

            return slots;
        }

        public async Task<List<SoftwareCatalogItemDto>> GetSoftwareCatalogAsync(short languageId = 12)
        {
            return await (
                from sw in _dbContext.Softwares
                where sw.IsActive
                join tr in _dbContext.Translations
                    on new { sw.CompanyId, LanguageId = languageId }
                    equals new { tr.CompanyId, tr.LanguageId }
                    into trJoin
                from tr in trJoin.DefaultIfEmpty()
                orderby (tr != null ? tr.Name : string.Empty), sw.Name
                select new SoftwareCatalogItemDto
                {
                    Id = sw.Id,
                    Name = sw.Name,
                    CompanyId = sw.CompanyId,
                    CompanyName = tr != null ? tr.Name : string.Empty
                }).AsNoTracking().ToListAsync();
        }

        public async Task UpsertAsync(Guid companyId, CompanySoftwareUpsertDto dto)
        {
            // The slot row is keyed by this company, so the target is also the stored row's company.
            await RequireCompanySoftwareModifyAsync(companyId);

            var existing = await _dbContext.CompanySoftwares
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.SoftwareCategoryId == dto.SoftwareCategoryId);
            if (existing != null)
            {
                existing.SoftwareId = dto.SoftwareId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.CompanySoftwares.Add(new CompanySoftware
                {
                    Id = Guid.CreateVersion7(),
                    CompanyId = companyId,
                    SoftwareCategoryId = dto.SoftwareCategoryId,
                    SoftwareId = dto.SoftwareId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task ClearAsync(Guid companyId, byte softwareCategoryId)
        {
            await RequireCompanySoftwareModifyAsync(companyId);

            var existing = await _dbContext.CompanySoftwares
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.SoftwareCategoryId == softwareCategoryId);
            if (existing != null)
            {
                _dbContext.CompanySoftwares.Remove(existing);
                await _dbContext.SaveChangesAsync();
            }
        }

        // The Information.Modify permission (checked by the controller attribute) is
        // global to the caller. This confirms the caller also holds Information level 2
        // on the specific company being changed. Fails closed: no caller, or a lower
        // level, is denied.
        private async Task RequireCompanySoftwareModifyAsync(Guid companyId)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, GetCallerPersonId());
            if (levels.Information < ModifyLevel)
                throw new BusinessRuleException(ModifyDenied);
        }

        private Guid GetCallerPersonId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var raw = user?.FindFirstValue("personId")
                   ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var personId))
                throw new BusinessRuleException("Caller PersonId not found on the JWT.");
            return personId;
        }
    }
}
