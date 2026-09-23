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
    public class FinancialInfoService : BaseService<FinancialInfo, FinancialInfoDto, FinancialInfoInsertDto, FinancialInfoDto>, IFinancialInfoService, ICompanyScopedWrites
    {
        private const int ModifyLevel = 2;
        private const string ModifyDenied = "You do not have permission to modify this company's financial information.";
        private const string MoveDenied = "Financial information cannot be moved to another company.";

        private readonly IFinancialInfoRepository _financialInfoRepository;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinancialInfoService(
            IMapper mapper,
            IFinancialInfoRepository repository,
            IAccessService accessService,
            IHttpContextAccessor httpContextAccessor) : base(mapper, repository)
        {
            _financialInfoRepository = repository;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Report read — a company's financial-info rows, no per-caller access filter.
        public async Task<List<FinancialInfoDto>> GetByCompanyAsync(Guid companyId)
        {
            var rows = await _financialInfoRepository.ToListAsync(x => x.CompanyId == companyId);
            return _mapper.Map<List<FinancialInfoDto>>(rows);
        }

        // Every write reachable through BaseController checks Information level 2 on the
        // company its rows belong to. Updates and removals use the STORED row, never the
        // company in the request body. Range operations check every item before anything
        // is written: one denied item denies the whole call.

        public override async Task AddAsync(FinancialInfo item, bool saveChanges = true)
        {
            await RequireFinancialInfoModifyAsync(item.CompanyId);
            await base.AddAsync(item, saveChanges);
        }

        public override async Task AddDtoAsync(FinancialInfoInsertDto item, bool saveChanges = true)
        {
            var entity = _mapper.Map<FinancialInfo>(item);
            await RequireFinancialInfoModifyAsync(entity.CompanyId);
            ValidateFinancialInfo(entity);
            await base.AddAsync(entity, saveChanges);
        }

        public override async Task AddRangeAsync(IEnumerable<FinancialInfo> items, bool saveChanges = true)
        {
            var list = items.ToList();
            await RequireModifyOnEveryCompanyAsync(list.Select(x => x.CompanyId));
            await base.AddRangeAsync(list, saveChanges);
        }

        public override void Update(FinancialInfo item, bool saveChanges = true)
        {
            var stored = PrepareUpdates(new[] { item });
            base.Update(stored[0], saveChanges);
        }

        public override void UpdateRange(IEnumerable<FinancialInfo> items, bool saveChanges = true)
        {
            base.UpdateRange(PrepareUpdates(items), saveChanges);
        }

        /// <summary>
        /// Load existing entity first, then only update the editable fields.
        /// This avoids DbUpdateConcurrencyException caused by AutoMapper setting
        /// CreatedAt/UpdatedAt to default values on a detached entity.
        /// </summary>
        public override void UpdateDto(FinancialInfoDto item, bool saveChanges = true)
        {
            var existing = _financialInfoRepository.Find(item.Id)
                ?? throw new KeyNotFoundException($"FinancialInfo with Id '{item.Id}' not found.");

            // The stored row's company decides; the row cannot be moved to another company.
            RequireFinancialInfoModifyAsync(existing.CompanyId).GetAwaiter().GetResult();
            if (item.CompanyId != Guid.Empty && item.CompanyId != existing.CompanyId)
                throw new BusinessRuleException(MoveDenied);

            // Only update the editable fields — preserve CreatedAt, UpdatedAt (managed by trigger)
            existing.FiscalYear = item.FiscalYear;
            existing.RegisteredCapital = item.RegisteredCapital;
            existing.NumberOfShares = item.NumberOfShares;

            ValidateFinancialInfo(existing);
            base.Update(existing, saveChanges);
        }

        public override void Remove(FinancialInfo item, bool saveChanges = true)
        {
            var stored = _financialInfoRepository.Find(item.Id);
            if (stored == null)
                return;

            RequireFinancialInfoModifyAsync(stored.CompanyId).GetAwaiter().GetResult();
            // The tracked stored row is removed, not the request instance.
            base.Remove(stored, saveChanges);
        }

        public override void RemoveRange(IEnumerable<FinancialInfo> items, bool saveChanges = true)
        {
            var stored = items
                .Select(x => _financialInfoRepository.Find(x.Id))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            RequireModifyOnEveryCompanyAsync(stored.Select(x => x.CompanyId)).GetAwaiter().GetResult();
            base.RemoveRange(stored, saveChanges);
        }

        private static void ValidateFinancialInfo(FinancialInfo info)
        {
            if (info.FiscalYear < 1300 || info.FiscalYear > 1500)
            {
                throw new ArgumentException("FiscalYear must be a valid Shamsi year (1300-1500).", nameof(info));
            }

            if (info.RegisteredCapital <= 0)
            {
                throw new ArgumentException("RegisteredCapital is required and must be greater than zero.", nameof(info));
            }

            if (info.NumberOfShares <= 0)
            {
                throw new ArgumentException("NumberOfShares is required and must be greater than zero.", nameof(info));
            }
        }

        // Loads every stored row, checks all of them (company level on the stored row, and
        // no change of company) before any value is applied, then copies the request's
        // values onto the tracked stored rows. Writing through the tracked instance avoids
        // attaching a second instance with the same key.
        private List<FinancialInfo> PrepareUpdates(IEnumerable<FinancialInfo> items)
        {
            var pairs = items
                .Select(item => (Item: item, Stored: _financialInfoRepository.Find(item.Id)
                    ?? throw new KeyNotFoundException($"FinancialInfo with Id '{item.Id}' not found.")))
                .ToList();

            if (pairs.Any(p => p.Item.CompanyId != p.Stored.CompanyId))
                throw new BusinessRuleException(MoveDenied);

            RequireModifyOnEveryCompanyAsync(pairs.Select(p => p.Stored.CompanyId)).GetAwaiter().GetResult();

            foreach (var (item, stored) in pairs)
            {
                CopyScalarValues(item, stored);
                ValidateFinancialInfo(stored);
            }

            return pairs.Select(p => p.Stored).ToList();
        }

        // Scalar columns only; navigation properties and the key are left untouched.
        private static void CopyScalarValues(FinancialInfo source, FinancialInfo target)
        {
            foreach (var property in typeof(FinancialInfo).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite || property.Name == nameof(FinancialInfo.Id))
                    continue;

                var type = property.PropertyType;
                if (type.IsValueType || type == typeof(string))
                    property.SetValue(target, property.GetValue(source));
            }
        }

        private async Task RequireModifyOnEveryCompanyAsync(IEnumerable<Guid> companyIds)
        {
            foreach (var companyId in companyIds.Distinct())
                await RequireFinancialInfoModifyAsync(companyId);
        }

        // The Information.Modify permission (checked by the permission filter) is global to
        // the caller. This confirms the caller also holds Information level 2 on the
        // specific company the rows belong to. Fails closed: no caller, or a lower level,
        // is denied.
        private async Task RequireFinancialInfoModifyAsync(Guid companyId)
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
