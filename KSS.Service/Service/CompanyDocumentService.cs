using System.Security.Claims;
using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Service
{
    public class CompanyDocumentService : BaseService<CompanyDocument, CompanyDocumentViewDto, CompanyDocumentInsertDto, CompanyDocumentUpdateDto>, ICompanyDocumentService
    {
        private const int ReadLevel = 1;
        private const int ModifyLevel = 2;
        private const string ReadDenied = "You do not have permission to view this company's documents.";
        private const string ModifyDenied = "You do not have permission to modify this company's documents.";
        private const string MoveDenied = "A document cannot be moved to another company.";

        private readonly ICompanyDocumentRepository _companyDocumentRepository;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyDocumentService(
            IMapper mapper,
            ICompanyDocumentRepository repository,
            IAccessService accessService,
            IHttpContextAccessor httpContextAccessor) : base(mapper, repository)
        {
            _companyDocumentRepository = repository;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        // A company's document metadata rows (newest first).
        public async Task<List<CompanyDocumentViewDto>> GetByCompanyAsync(Guid companyId)
        {
            await RequireCompanyDocumentAccessAsync(companyId, ReadLevel);

            var rows = await _companyDocumentRepository.ToListAsync(x => x.CompanyId == companyId);
            return _mapper.Map<List<CompanyDocumentViewDto>>(rows.OrderByDescending(x => x.CreatedAt));
        }

        // Create from an InsertDto and return the created row (Id/CreatedAt are
        // stamped by MainDbContext.ApplyEntityDefaults on SaveChanges, so the
        // mapped view carries the new GUID id back to the caller).
        public async Task<CompanyDocumentViewDto> CreateAsync(CompanyDocumentInsertDto dto)
        {
            await RequireCompanyDocumentAccessAsync(dto.CompanyId, ModifyLevel);

            var entity = _mapper.Map<CompanyDocument>(dto);
            // Already checked above; persist through the base implementation.
            await base.AddAsync(entity, true);
            return _mapper.Map<CompanyDocumentViewDto>(entity);
        }

        // Delete by key — no full-entity bind (whose non-nullable nav props would
        // otherwise be implicitly required under [ApiController] + nullable refs).
        // Authorised against the stored row's company, never a value from the request.
        public async Task DeleteByIdAsync(Guid id)
        {
            var existing = _companyDocumentRepository.Find(id);
            if (existing == null)
                return;

            await RequireCompanyDocumentAccessAsync(existing.CompanyId, ModifyLevel);
            // Already checked above; persist through the base implementation.
            base.Remove(existing);
        }

        /// <summary>
        /// Load the existing row first, then patch only the editable fields
        /// (document type + file name). Preserves CompanyId, StorageInstanceId,
        /// FileSize, ContentType and the audit columns — mirrors FinancialInfoService.
        /// The update DTO carries no CompanyId, so a row cannot be moved to another company.
        /// </summary>
        public override void UpdateDto(CompanyDocumentUpdateDto item, bool saveChanges = true)
        {
            var existing = _companyDocumentRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"CompanyDocument with Id '{item.Id}' not found.");

            // The inherited UpdateDto entry point is synchronous, so the check is awaited here.
            RequireCompanyDocumentAccessAsync(existing.CompanyId, ModifyLevel).GetAwaiter().GetResult();

            existing.CompanyDocumentTypeId = item.CompanyDocumentTypeId;
            // FileName is refreshed only when the BFF sends a non-empty value
            // (i.e. a new file was uploaded); keep the current name otherwise.
            if (!string.IsNullOrWhiteSpace(item.FileName))
                existing.FileName = item.FileName;

            base.Update(existing, saveChanges);
        }

        // ── Inherited CRUD writes ───────────────────────────────────────────────
        // BaseController exposes these for every entity. Each is checked against the
        // company its rows belong to. Updates and removals use the STORED row, never the
        // company in the request body. Range operations check every item before anything
        // is written: one denied item denies the whole call.

        public override async Task AddAsync(CompanyDocument item, bool saveChanges = true)
        {
            await RequireCompanyDocumentAccessAsync(item.CompanyId, ModifyLevel);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(CompanyDocumentInsertDto item, bool saveChanges = true)
        {
            await RequireCompanyDocumentAccessAsync(item.CompanyId, ModifyLevel);
            await base.AddDtoAsync(item, saveChanges);
        }

        public override async Task AddRangeAsync(IEnumerable<CompanyDocument> items, bool saveChanges = true)
        {
            var list = items.ToList();
            await RequireModifyOnEveryCompanyAsync(list.Select(x => x.CompanyId));
            await base.AddRangeAsync(list, saveChanges);
        }

        public override void Update(CompanyDocument item, bool saveChanges = true)
        {
            var stored = PrepareUpdates(new[] { item });
            base.Update(stored[0], saveChanges);
        }

        public override void UpdateRange(IEnumerable<CompanyDocument> items, bool saveChanges = true)
        {
            base.UpdateRange(PrepareUpdates(items), saveChanges);
        }

        public override void Remove(CompanyDocument item, bool saveChanges = true)
        {
            var stored = _companyDocumentRepository.Find(item.Id);
            if (stored == null)
                return;

            RequireCompanyDocumentAccessAsync(stored.CompanyId, ModifyLevel).GetAwaiter().GetResult();
            // The tracked stored row is removed, not the request instance.
            base.Remove(stored, saveChanges);
        }

        public override void RemoveRange(IEnumerable<CompanyDocument> items, bool saveChanges = true)
        {
            var stored = items
                .Select(x => _companyDocumentRepository.Find(x.Id))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            RequireModifyOnEveryCompanyAsync(stored.Select(x => x.CompanyId)).GetAwaiter().GetResult();
            base.RemoveRange(stored, saveChanges);
        }

        // Loads every stored row, checks all of them (company level on the stored row, and
        // no change of company) before any value is applied, then copies the request's
        // values onto the tracked stored rows. Writing through the tracked instance avoids
        // attaching a second instance with the same key.
        private List<CompanyDocument> PrepareUpdates(IEnumerable<CompanyDocument> items)
        {
            var pairs = items
                .Select(item => (Item: item, Stored: _companyDocumentRepository.Find(item.Id)
                    ?? throw new KeyNotFoundException($"CompanyDocument with Id '{item.Id}' not found.")))
                .ToList();

            if (pairs.Any(p => p.Item.CompanyId != p.Stored.CompanyId))
                throw new BusinessRuleException(MoveDenied);

            RequireModifyOnEveryCompanyAsync(pairs.Select(p => p.Stored.CompanyId)).GetAwaiter().GetResult();

            foreach (var (item, stored) in pairs)
                CopyScalarValues(item, stored);

            return pairs.Select(p => p.Stored).ToList();
        }

        // Scalar columns only; navigation properties and the key are left untouched.
        private static void CopyScalarValues(CompanyDocument source, CompanyDocument target)
        {
            foreach (var property in typeof(CompanyDocument).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite || property.Name == nameof(CompanyDocument.Id))
                    continue;

                var type = property.PropertyType;
                if (type.IsValueType || type == typeof(string))
                    property.SetValue(target, property.GetValue(source));
            }
        }

        private async Task RequireModifyOnEveryCompanyAsync(IEnumerable<Guid> companyIds)
        {
            foreach (var companyId in companyIds.Distinct())
                await RequireCompanyDocumentAccessAsync(companyId, ModifyLevel);
        }

        // A section permission (checked by the controller attribute) is global to the
        // caller. This confirms the caller also holds the required Information level on
        // the specific company the rows belong to. Fails closed: no caller, or a level
        // below the minimum, is denied.
        private async Task RequireCompanyDocumentAccessAsync(Guid companyId, int minimumLevel)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, GetCallerPersonId());
            if (levels.Information < minimumLevel)
                throw new BusinessRuleException(minimumLevel >= ModifyLevel ? ModifyDenied : ReadDenied);
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
