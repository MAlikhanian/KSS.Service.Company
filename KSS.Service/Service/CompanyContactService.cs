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
    public class CompanyContactService : ICompanyContactService
    {
        private const int ModifyLevel = 2;
        private const string ModifyDenied = "You do not have permission to modify this company's contact details.";
        private const string MoveDenied = "A contact entry cannot be moved to another company.";

        private readonly MainDbContext _dbContext;
        private readonly IAccessService _accessService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyContactService(MainDbContext dbContext, IAccessService accessService, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _accessService = accessService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CompanyContactDto?> GetContactDataAsync(Guid companyId, Guid callerPersonId, short languageId = 12)
        {
            // Row-level access gate. Caller must have Information.Read (>=1) on
            // THIS company. Null lets the controller respond 404 without leaking
            // existence.
            var levels = await _accessService.GetLevelsAsync(companyId, callerPersonId);
            if (levels.Information < 1) return null;

            // Emails with label names
            var emails = await (from e in _dbContext.Emails
                                where e.CompanyId == companyId
                                join lt in _dbContext.EmailLabelTranslations
                                    on new { e.LabelId, LanguageId = languageId }
                                    equals new { LabelId = lt.EmailLabelId, lt.LanguageId }
                                    into labelJoin
                                from lt in labelJoin.DefaultIfEmpty()
                                orderby e.IsPrimary descending, e.EmailAddress
                                select new CompanyEmailViewDto
                                {
                                    Id = e.Id,
                                    CompanyId = e.CompanyId,
                                    LabelId = e.LabelId,
                                    LabelName = lt != null ? lt.Name : string.Empty,
                                    EmailAddress = e.EmailAddress,
                                    IsPrimary = e.IsPrimary,
                                    IsVerified = e.IsVerified,
                                    CreatedAt = e.CreatedAt,
                                    UpdatedAt = e.UpdatedAt
                                }).AsNoTracking().ToListAsync();

            // Phones with label names
            var phones = await (from p in _dbContext.Phones
                                where p.CompanyId == companyId
                                join lt in _dbContext.PhoneLabelTranslations
                                    on new { p.LabelId, LanguageId = languageId }
                                    equals new { LabelId = lt.PhoneLabelId, lt.LanguageId }
                                    into labelJoin
                                from lt in labelJoin.DefaultIfEmpty()
                                orderby p.IsPrimary descending, p.PhoneNumber
                                select new CompanyPhoneViewDto
                                {
                                    Id = p.Id,
                                    CompanyId = p.CompanyId,
                                    LabelId = p.LabelId,
                                    LabelName = lt != null ? lt.Name : string.Empty,
                                    CountryId = p.CountryId,
                                    PhoneNumber = p.PhoneNumber,
                                    IsPrimary = p.IsPrimary,
                                    IsVerified = p.IsVerified,
                                    CreatedAt = p.CreatedAt,
                                    UpdatedAt = p.UpdatedAt
                                }).AsNoTracking().ToListAsync();

            // Addresses with label names and translations
            var addresses = await (from a in _dbContext.Addresses
                                   where a.CompanyId == companyId
                                   join lt in _dbContext.AddressLabelTranslations
                                       on new { a.LabelId, LanguageId = languageId }
                                       equals new { LabelId = lt.AddressLabelId, lt.LanguageId }
                                       into labelJoin
                                   from lt in labelJoin.DefaultIfEmpty()
                                   join at in _dbContext.AddressTranslations
                                       on new { AddressId = a.Id, LanguageId = languageId }
                                       equals new { at.AddressId, at.LanguageId }
                                       into transJoin
                                   from at in transJoin.DefaultIfEmpty()
                                   orderby a.IsPrimary descending
                                   select new CompanyAddressViewDto
                                   {
                                       Id = a.Id,
                                       CompanyId = a.CompanyId,
                                       LabelId = a.LabelId,
                                       LabelName = lt != null ? lt.Name : string.Empty,
                                       CountryId = a.CountryId,
                                       RegionId = a.RegionId,
                                       CityId = a.CityId,
                                       PostalCode = a.PostalCode,
                                       Street1 = at != null ? at.Street1 : string.Empty,
                                       Street2 = at != null ? at.Street2 : null,
                                       IsPrimary = a.IsPrimary,
                                       IsVerified = a.IsVerified,
                                       CreatedAt = a.CreatedAt,
                                       UpdatedAt = a.UpdatedAt
                                   }).AsNoTracking().ToListAsync();

            // Websites with label names
            var websites = await (from w in _dbContext.Websites
                                  where w.CompanyId == companyId
                                  join lt in _dbContext.WebsiteLabelTranslations
                                      on new { w.LabelId, LanguageId = languageId }
                                      equals new { LabelId = lt.WebsiteLabelId, lt.LanguageId }
                                      into labelJoin
                                  from lt in labelJoin.DefaultIfEmpty()
                                  orderby w.IsPrimary descending, w.Url
                                  select new CompanyWebsiteViewDto
                                  {
                                      Id = w.Id,
                                      CompanyId = w.CompanyId,
                                      LabelId = w.LabelId,
                                      LabelName = lt != null ? lt.Name : string.Empty,
                                      Url = w.Url,
                                      IsPrimary = w.IsPrimary,
                                      CreatedAt = w.CreatedAt,
                                      UpdatedAt = w.UpdatedAt
                                  }).AsNoTracking().ToListAsync();

            return new CompanyContactDto
            {
                Emails = emails,
                Phones = phones,
                Addresses = addresses,
                Websites = websites
            };
        }

        public async Task<CompanyEmailViewDto> AddEmailAsync(Guid companyId, CompanyEmailInsertDto dto)
        {
            await RequireCompanyContactModifyAsync(companyId);

            var entity = new Email
            {
                Id = Guid.CreateVersion7(),
                CompanyId = companyId,
                LabelId = dto.LabelId,
                EmailAddress = dto.EmailAddress.Trim().ToLowerInvariant(),
                IsPrimary = dto.IsPrimary,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Emails.Add(entity);
            await _dbContext.SaveChangesAsync();

            return new CompanyEmailViewDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                LabelId = entity.LabelId,
                LabelName = string.Empty,
                EmailAddress = entity.EmailAddress,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        public async Task<CompanyEmailViewDto> UpdateEmailAsync(Guid emailId, CompanyEmailViewDto dto)
        {
            var entity = await _dbContext.Emails.FindAsync(emailId)
                ?? throw new KeyNotFoundException($"Email {emailId} not found");
            await RequireStoredRowModifyAsync(entity.CompanyId, dto.CompanyId);
            entity.LabelId = dto.LabelId;
            entity.EmailAddress = dto.EmailAddress.Trim().ToLowerInvariant();
            entity.IsPrimary = dto.IsPrimary;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CompanyId = entity.CompanyId;
            return dto;
        }

        public async Task DeleteEmailAsync(Guid emailId)
        {
            var entity = await _dbContext.Emails.FindAsync(emailId);
            if (entity != null)
            {
                await RequireCompanyContactModifyAsync(entity.CompanyId);
                _dbContext.Emails.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CompanyPhoneViewDto> AddPhoneAsync(Guid companyId, CompanyPhoneInsertDto dto)
        {
            await RequireCompanyContactModifyAsync(companyId);

            var entity = new Phone
            {
                Id = Guid.CreateVersion7(),
                CompanyId = companyId,
                LabelId = dto.LabelId,
                CountryId = dto.CountryId,
                PhoneNumber = dto.PhoneNumber.Trim(),
                IsPrimary = dto.IsPrimary,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Phones.Add(entity);
            await _dbContext.SaveChangesAsync();

            return new CompanyPhoneViewDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                LabelId = entity.LabelId,
                LabelName = string.Empty,
                CountryId = entity.CountryId,
                PhoneNumber = entity.PhoneNumber,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        public async Task<CompanyPhoneViewDto> UpdatePhoneAsync(Guid phoneId, CompanyPhoneViewDto dto)
        {
            var entity = await _dbContext.Phones.FindAsync(phoneId)
                ?? throw new KeyNotFoundException($"Phone {phoneId} not found");
            await RequireStoredRowModifyAsync(entity.CompanyId, dto.CompanyId);
            entity.LabelId = dto.LabelId;
            entity.CountryId = dto.CountryId;
            entity.PhoneNumber = dto.PhoneNumber.Trim();
            entity.IsPrimary = dto.IsPrimary;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CompanyId = entity.CompanyId;
            return dto;
        }

        public async Task DeletePhoneAsync(Guid phoneId)
        {
            var entity = await _dbContext.Phones.FindAsync(phoneId);
            if (entity != null)
            {
                await RequireCompanyContactModifyAsync(entity.CompanyId);
                _dbContext.Phones.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CompanyAddressViewDto> AddAddressAsync(Guid companyId, CompanyAddressInsertDto dto, short languageId = 12)
        {
            await RequireCompanyContactModifyAsync(companyId);

            var entity = new Address
            {
                Id = Guid.CreateVersion7(),
                CompanyId = companyId,
                LabelId = dto.LabelId,
                CountryId = dto.CountryId,
                RegionId = dto.RegionId,
                CityId = dto.CityId,
                PostalCode = dto.PostalCode.Trim(),
                IsPrimary = dto.IsPrimary,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Addresses.Add(entity);

            // Add translation for the street
            if (!string.IsNullOrWhiteSpace(dto.Street1))
            {
                _dbContext.AddressTranslations.Add(new AddressTranslation
                {
                    AddressId = entity.Id,
                    LanguageId = languageId,
                    Street1 = dto.Street1.Trim(),
                    Street2 = dto.Street2?.Trim()
                });
            }

            await _dbContext.SaveChangesAsync();

            return new CompanyAddressViewDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                LabelId = entity.LabelId,
                LabelName = string.Empty,
                CountryId = entity.CountryId,
                RegionId = entity.RegionId,
                CityId = entity.CityId,
                PostalCode = entity.PostalCode,
                Street1 = dto.Street1,
                Street2 = dto.Street2,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        public async Task<CompanyAddressViewDto> UpdateAddressAsync(Guid addressId, CompanyAddressViewDto dto, short languageId = 12)
        {
            var entity = await _dbContext.Addresses.FindAsync(addressId)
                ?? throw new KeyNotFoundException($"Address {addressId} not found");
            await RequireStoredRowModifyAsync(entity.CompanyId, dto.CompanyId);
            entity.LabelId = dto.LabelId;
            entity.CountryId = dto.CountryId;
            entity.RegionId = dto.RegionId;
            entity.CityId = dto.CityId;
            entity.PostalCode = dto.PostalCode.Trim();
            entity.IsPrimary = dto.IsPrimary;
            entity.UpdatedAt = DateTime.UtcNow;

            // Update or insert address translation (Street1, Street2)
            var translation = await _dbContext.AddressTranslations
                .FirstOrDefaultAsync(at => at.AddressId == addressId && at.LanguageId == languageId);

            if (translation != null)
            {
                translation.Street1 = dto.Street1?.Trim() ?? string.Empty;
                translation.Street2 = dto.Street2?.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(dto.Street1))
            {
                _dbContext.AddressTranslations.Add(new AddressTranslation
                {
                    AddressId = addressId,
                    LanguageId = languageId,
                    Street1 = dto.Street1.Trim(),
                    Street2 = dto.Street2?.Trim()
                });
            }

            await _dbContext.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CompanyId = entity.CompanyId;
            return dto;
        }

        public async Task DeleteAddressAsync(Guid addressId)
        {
            // The stored address decides the company, so it is loaded and checked before
            // anything is removed. Without it there is no company to check against.
            var entity = await _dbContext.Addresses.FindAsync(addressId);
            if (entity == null) return;
            await RequireCompanyContactModifyAsync(entity.CompanyId);

            var translations = await _dbContext.AddressTranslations
                .Where(at => at.AddressId == addressId).ToListAsync();
            _dbContext.AddressTranslations.RemoveRange(translations);
            _dbContext.Addresses.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CompanyWebsiteViewDto> AddWebsiteAsync(Guid companyId, CompanyWebsiteInsertDto dto)
        {
            await RequireCompanyContactModifyAsync(companyId);

            var entity = new Website
            {
                Id = Guid.CreateVersion7(),
                CompanyId = companyId,
                LabelId = dto.LabelId,
                Url = dto.Url.Trim(),
                IsPrimary = dto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Websites.Add(entity);
            await _dbContext.SaveChangesAsync();

            return new CompanyWebsiteViewDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                LabelId = entity.LabelId,
                LabelName = string.Empty,
                Url = entity.Url,
                IsPrimary = entity.IsPrimary,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        public async Task<CompanyWebsiteViewDto> UpdateWebsiteAsync(Guid websiteId, CompanyWebsiteViewDto dto)
        {
            var entity = await _dbContext.Websites.FindAsync(websiteId)
                ?? throw new KeyNotFoundException($"Website {websiteId} not found");
            await RequireStoredRowModifyAsync(entity.CompanyId, dto.CompanyId);
            entity.LabelId = dto.LabelId;
            entity.Url = dto.Url.Trim();
            entity.IsPrimary = dto.IsPrimary;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CompanyId = entity.CompanyId;
            return dto;
        }

        public async Task DeleteWebsiteAsync(Guid websiteId)
        {
            var entity = await _dbContext.Websites.FindAsync(websiteId);
            if (entity != null)
            {
                await RequireCompanyContactModifyAsync(entity.CompanyId);
                _dbContext.Websites.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        // An update is checked against the company the stored row belongs to, never a
        // company named in the request, and cannot move the row to another company.
        private async Task RequireStoredRowModifyAsync(Guid storedCompanyId, Guid requestedCompanyId)
        {
            await RequireCompanyContactModifyAsync(storedCompanyId);
            if (requestedCompanyId != Guid.Empty && requestedCompanyId != storedCompanyId)
                throw new BusinessRuleException(MoveDenied);
        }

        // The Information.Modify permission (checked by the controller attribute) is
        // global to the caller. This confirms the caller also holds Information level 2
        // on the specific company being changed. Fails closed: no caller, or a lower
        // level, is denied.
        private async Task RequireCompanyContactModifyAsync(Guid companyId)
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
