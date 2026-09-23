using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Service.IService;
using KSS.Service.Client;

namespace KSS.Service.Service
{
    /// <summary>
    /// Orchestrates Stakeholder + StakeholderHistory + StakeholderType across
    /// multiple tables. Read returns hydrated rows with type translation and full
    /// history per stakeholder. Updates use append-history semantics: the prior
    /// current row is closed and a new current row is appended whenever any
    /// history-shaped field changes.
    /// </summary>
    public class CompanyStakeholderManagementService : ICompanyStakeholderManagementService
    {
        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;
        private readonly IPersonApiClient _personApi;

        public CompanyStakeholderManagementService(MainDbContext dbContext, IAccessService accessService, IPersonApiClient personApi)
        {
            _dbContext = dbContext;
            _accessService = accessService;
            _personApi = personApi;
        }

        public async Task<List<CompanyStakeholderViewDto>?> GetByCompanyAsync(Guid companyId, Guid callerPersonId, short languageId = 12)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, callerPersonId);
            if (levels.Information < 1) return null;

            var stakeholders = await (from s in _dbContext.Set<Stakeholder>()
                                      where s.CompanyId == companyId && s.IsActive
                                      join tt in _dbContext.Set<StakeholderTypeTranslation>()
                                          on new { TypeId = s.StakeholderTypeId, LanguageId = languageId }
                                          equals new { TypeId = tt.StakeholderTypeId, tt.LanguageId }
                                          into ttJoin
                                      from tt in ttJoin.DefaultIfEmpty()
                                      select new
                                      {
                                          s.Id,
                                          s.CompanyId,
                                          s.RelatedPartyType,
                                          s.RelatedPartyId,
                                          s.StakeholderTypeId,
                                          TypeName = tt != null ? tt.Name : string.Empty
                                      }).AsNoTracking().ToListAsync();

            if (stakeholders.Count == 0) return new List<CompanyStakeholderViewDto>();

            var stakeholderIds = stakeholders.Select(x => x.Id).ToList();

            var histories = await _dbContext.Set<StakeholderHistory>()
                .AsNoTracking()
                .Where(h => stakeholderIds.Contains(h.CompanyStakeholderId) && h.IsActive)
                .OrderByDescending(h => h.EffectiveDate)
                .ToListAsync();

            var historiesByStakeholder = histories
                .GroupBy(h => h.CompanyStakeholderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── Resolve display names server-side (no client-side join) ──
            // Person names (related-party type 2 + board representatives) come
            // from the Person service in ONE batched call; company names
            // (type 1) are resolved locally from the Translation table.
            var personIds = new HashSet<Guid>();
            foreach (var s in stakeholders)
                if (s.RelatedPartyType == 2) personIds.Add(s.RelatedPartyId);
            foreach (var h in histories)
                if (h.BoardRepresentativePersonId.HasValue) personIds.Add(h.BoardRepresentativePersonId.Value);

            // GetPersonNamesAsync short-circuits to an empty map for an empty set
            // and degrades gracefully (empty map) if the Person service errors.
            var personNames = await _personApi.GetPersonNamesAsync(personIds.ToList(), languageId);

            var companyIds = stakeholders
                .Where(s => s.RelatedPartyType == 1)
                .Select(s => s.RelatedPartyId)
                .Distinct()
                .ToList();

            // Company names in any language, chosen in memory: requested language first,
            // then any other available language; the registry id only when none has a name.
            var companyNames = new Dictionary<Guid, string>();
            if (companyIds.Count > 0)
            {
                var companies = await _dbContext.Companies
                    .Where(c => companyIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.NationalId })
                    .AsNoTracking()
                    .ToListAsync();

                var translationsByCompany = (await _dbContext.Translations
                        .Where(t => companyIds.Contains(t.CompanyId))
                        .Select(t => new { t.CompanyId, t.LanguageId, t.Name })
                        .AsNoTracking()
                        .ToListAsync())
                    .ToLookup(t => t.CompanyId);

                foreach (var c in companies)
                {
                    companyNames[c.Id] = CompanyDisplayName.PickCompanyDisplayName(
                        translationsByCompany[c.Id].Select(t => (t.LanguageId, (string?)t.Name)),
                        languageId,
                        c.NationalId ?? string.Empty);
                }
            }

            CompanyStakeholderHistoryViewDto MapHistory(StakeholderHistory h) => new()
            {
                Id = h.Id,
                OwnershipPercentage = h.OwnershipPercentage,
                ShareCount = h.ShareCount,
                BoardRepresentativePersonId = h.BoardRepresentativePersonId,
                BoardRepresentativeName = h.BoardRepresentativePersonId.HasValue
                    && personNames.TryGetValue(h.BoardRepresentativePersonId.Value, out var brn) ? brn : null,
                RegistrationDate = h.RegistrationDate,
                EffectiveDate = h.EffectiveDate,
                EndDate = h.EndDate,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
            };

            string ResolveRelatedPartyName(byte type, Guid id) =>
                type == 1
                    ? (companyNames.TryGetValue(id, out var cn) ? cn : string.Empty)
                    : (personNames.TryGetValue(id, out var pn) ? pn : string.Empty);

            return stakeholders.Select(s =>
            {
                var rows = historiesByStakeholder.TryGetValue(s.Id, out var list)
                    ? list.Select(MapHistory).ToList()
                    : new List<CompanyStakeholderHistoryViewDto>();

                return new CompanyStakeholderViewDto
                {
                    Id = s.Id,
                    CompanyId = s.CompanyId,
                    RelatedPartyType = s.RelatedPartyType,
                    RelatedPartyId = s.RelatedPartyId,
                    RelatedPartyName = ResolveRelatedPartyName(s.RelatedPartyType, s.RelatedPartyId),
                    StakeholderTypeId = s.StakeholderTypeId,
                    StakeholderTypeName = s.TypeName,
                    Current = rows.FirstOrDefault(r => r.EndDate == null),
                    History = rows,
                };
            }).ToList();
        }

        public async Task<CompanyStakeholderViewDto> AddAsync(Guid companyId, Guid callerPersonId, CompanyStakeholderUpsertDto dto, short languageId = 12)
        {
            var levels = await _accessService.GetLevelsAsync(companyId, callerPersonId);
            if (levels.Information < 2)
                throw new BusinessRuleException("You do not have permission to modify this company.");

            Validate(companyId, dto);

            var stakeholder = new Stakeholder
            {
                CompanyId = companyId,
                RelatedPartyType = dto.RelatedPartyType,
                RelatedPartyId = dto.RelatedPartyId,
                StakeholderTypeId = dto.StakeholderTypeId,
                IsActive = true,
            };
            _dbContext.Add(stakeholder);
            await _dbContext.SaveChangesAsync();

            var history = new StakeholderHistory
            {
                CompanyStakeholderId = stakeholder.Id,
                OwnershipPercentage = dto.OwnershipPercentage,
                ShareCount = dto.ShareCount,
                BoardRepresentativePersonId = dto.BoardRepresentativePersonId,
                RegistrationDate = dto.RegistrationDate.Date,
                EffectiveDate = dto.EffectiveDate.Date,
                EndDate = null,
                IsActive = true,
            };
            _dbContext.Add(history);
            await _dbContext.SaveChangesAsync();

            var view = (await GetByCompanyAsync(companyId, callerPersonId, languageId))
                ?.FirstOrDefault(v => v.Id == stakeholder.Id);
            return view ?? throw new BusinessRuleException("Failed to load the created stakeholder.");
        }

        public async Task<CompanyStakeholderViewDto> UpdateAsync(Guid stakeholderId, Guid callerPersonId, CompanyStakeholderUpsertDto dto, short languageId = 12)
        {
            var stakeholder = await _dbContext.Set<Stakeholder>()
                .FirstOrDefaultAsync(s => s.Id == stakeholderId && s.IsActive)
                ?? throw new BusinessRuleException("Stakeholder not found.");

            var levels = await _accessService.GetLevelsAsync(stakeholder.CompanyId, callerPersonId);
            if (levels.Information < 2)
                throw new BusinessRuleException("You do not have permission to modify this company.");

            Validate(stakeholder.CompanyId, dto);

            stakeholder.RelatedPartyType = dto.RelatedPartyType;
            stakeholder.RelatedPartyId = dto.RelatedPartyId;
            stakeholder.StakeholderTypeId = dto.StakeholderTypeId;
            _dbContext.Update(stakeholder);

            var current = await _dbContext.Set<StakeholderHistory>()
                .FirstOrDefaultAsync(h => h.CompanyStakeholderId == stakeholderId && h.IsActive && h.EndDate == null);

            if (current == null)
            {
                // No current row yet — insert one.
                _dbContext.Add(new StakeholderHistory
                {
                    CompanyStakeholderId = stakeholderId,
                    OwnershipPercentage = dto.OwnershipPercentage,
                    ShareCount = dto.ShareCount,
                    BoardRepresentativePersonId = dto.BoardRepresentativePersonId,
                    RegistrationDate = dto.RegistrationDate.Date,
                    EffectiveDate = dto.EffectiveDate.Date,
                    EndDate = null,
                    IsActive = true,
                });
            }
            else if (current.EffectiveDate.Date != dto.EffectiveDate.Date)
            {
                // Effective date moved — record a new history period. Close the
                // previous current row and insert a new one. UX_StakeholderHistory
                // _Stakeholder_EffectiveDate makes (stakeholder, EffectiveDate)
                // unique, so this branch is the only safe way to introduce a new
                // period for the same stakeholder.
                current.EndDate = DateTime.UtcNow.Date;
                _dbContext.Update(current);

                _dbContext.Add(new StakeholderHistory
                {
                    CompanyStakeholderId = stakeholderId,
                    OwnershipPercentage = dto.OwnershipPercentage,
                    ShareCount = dto.ShareCount,
                    BoardRepresentativePersonId = dto.BoardRepresentativePersonId,
                    RegistrationDate = dto.RegistrationDate.Date,
                    EffectiveDate = dto.EffectiveDate.Date,
                    EndDate = null,
                    IsActive = true,
                });
            }
            else if (current.OwnershipPercentage != dto.OwnershipPercentage
                  || current.ShareCount != dto.ShareCount
                  || current.BoardRepresentativePersonId != dto.BoardRepresentativePersonId
                  || current.RegistrationDate.Date != dto.RegistrationDate.Date)
            {
                // Same EffectiveDate — caller is correcting fields on the current
                // row, not recording a new period. Update in place.
                current.OwnershipPercentage = dto.OwnershipPercentage;
                current.ShareCount = dto.ShareCount;
                current.BoardRepresentativePersonId = dto.BoardRepresentativePersonId;
                current.RegistrationDate = dto.RegistrationDate.Date;
                _dbContext.Update(current);
            }

            await _dbContext.SaveChangesAsync();

            var view = (await GetByCompanyAsync(stakeholder.CompanyId, callerPersonId, languageId))
                ?.FirstOrDefault(v => v.Id == stakeholderId);
            return view ?? throw new BusinessRuleException("Failed to load the updated stakeholder.");
        }

        public async Task DeleteAsync(Guid stakeholderId, Guid callerPersonId)
        {
            var stakeholder = await _dbContext.Set<Stakeholder>()
                .FirstOrDefaultAsync(s => s.Id == stakeholderId && s.IsActive);
            if (stakeholder == null) return;

            var levels = await _accessService.GetLevelsAsync(stakeholder.CompanyId, callerPersonId);
            if (levels.Information < 2)
                throw new BusinessRuleException("You do not have permission to modify this company.");

            var now = DateTime.UtcNow;
            stakeholder.IsActive = false;
            stakeholder.DeletedAt = now;
            stakeholder.DeletedBy = callerPersonId;
            _dbContext.Update(stakeholder);

            var histories = await _dbContext.Set<StakeholderHistory>()
                .Where(h => h.CompanyStakeholderId == stakeholderId && h.IsActive)
                .ToListAsync();
            foreach (var h in histories)
            {
                h.IsActive = false;
                h.DeletedAt = now;
                h.DeletedBy = callerPersonId;
            }
            _dbContext.UpdateRange(histories);

            await _dbContext.SaveChangesAsync();
        }

        private static void Validate(Guid companyId, CompanyStakeholderUpsertDto dto)
        {
            if (dto.RelatedPartyType != 1 && dto.RelatedPartyType != 2)
                throw new BusinessRuleException("RelatedPartyType must be 1 (Company) or 2 (Person).");

            if (dto.RelatedPartyType == 1 && dto.RelatedPartyId == companyId)
                throw new BusinessRuleException("A company cannot be a stakeholder of itself.");

            if (dto.OwnershipPercentage < 0 || dto.OwnershipPercentage > 100)
                throw new BusinessRuleException("Ownership percentage must be between 0 and 100.");

            if (dto.ShareCount < 0)
                throw new BusinessRuleException("Share count cannot be negative.");

            if (dto.EffectiveDate.Date < dto.RegistrationDate.Date)
                throw new BusinessRuleException("EFFECTIVE_DATE_BEFORE_REGISTRATION");
        }
    }
}
