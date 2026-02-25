using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CompanyContactService : ICompanyContactService
    {
        private readonly MainDbContext _dbContext;

        public CompanyContactService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CompanyContactDto> GetContactDataAsync(Guid companyId, short languageId = 12)
        {
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
                                    IsVerified = e.IsVerified
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
                                    IsVerified = p.IsVerified
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
                                       IsVerified = a.IsVerified
                                   }).AsNoTracking().ToListAsync();

            return new CompanyContactDto
            {
                Emails = emails,
                Phones = phones,
                Addresses = addresses
            };
        }

        public async Task<CompanyEmailViewDto> AddEmailAsync(Guid companyId, CompanyEmailViewDto dto)
        {
            var entity = new Email
            {
                Id = Guid.NewGuid(),
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

            dto.Id = entity.Id;
            dto.CompanyId = companyId;
            return dto;
        }

        public async Task<CompanyEmailViewDto> UpdateEmailAsync(Guid emailId, CompanyEmailViewDto dto)
        {
            var entity = await _dbContext.Emails.FindAsync(emailId)
                ?? throw new KeyNotFoundException($"Email {emailId} not found");
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
                _dbContext.Emails.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CompanyPhoneViewDto> AddPhoneAsync(Guid companyId, CompanyPhoneViewDto dto)
        {
            var entity = new Phone
            {
                Id = Guid.NewGuid(),
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

            dto.Id = entity.Id;
            dto.CompanyId = companyId;
            return dto;
        }

        public async Task<CompanyPhoneViewDto> UpdatePhoneAsync(Guid phoneId, CompanyPhoneViewDto dto)
        {
            var entity = await _dbContext.Phones.FindAsync(phoneId)
                ?? throw new KeyNotFoundException($"Phone {phoneId} not found");
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
                _dbContext.Phones.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CompanyAddressViewDto> AddAddressAsync(Guid companyId, CompanyAddressViewDto dto, short languageId = 12)
        {
            var entity = new Address
            {
                Id = Guid.NewGuid(),
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

            dto.Id = entity.Id;
            dto.CompanyId = companyId;
            return dto;
        }

        public async Task<CompanyAddressViewDto> UpdateAddressAsync(Guid addressId, CompanyAddressViewDto dto, short languageId = 12)
        {
            var entity = await _dbContext.Addresses.FindAsync(addressId)
                ?? throw new KeyNotFoundException($"Address {addressId} not found");
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
            var translations = await _dbContext.AddressTranslations
                .Where(at => at.AddressId == addressId).ToListAsync();
            _dbContext.AddressTranslations.RemoveRange(translations);

            var entity = await _dbContext.Addresses.FindAsync(addressId);
            if (entity != null)
            {
                _dbContext.Addresses.Remove(entity);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
