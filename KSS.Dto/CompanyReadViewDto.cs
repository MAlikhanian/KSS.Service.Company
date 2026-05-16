namespace KSS.Dto
{
    /// <summary>
    /// Read-only consolidated view of a company for display screens
    /// (e.g. the brokerage general-information page where Company data is shown
    /// alongside ERP/SEO data managed by a separate service).
    ///
    /// Returns IDs only for country/region/city — those lookup tables live in
    /// KSS.Service.Common, so name resolution is done in the BFF layer, not here.
    /// </summary>
    public class CompanyReadViewDto
    {
        public Guid Id { get; set; }

        // From Translation (LanguageId = 12 Persian)
        public string CompanyPersianName { get; set; } = string.Empty;

        // From Translation (LanguageId = 10 English)
        public string? CompanyLatinName { get; set; }

        // From Company
        public DateTime RegistrationDate { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string EconomicCode { get; set; } = string.Empty;
        public short RegistrationCountryId { get; set; }
        public short RegistrationRegionId { get; set; }
        public int RegistrationCityId { get; set; }
        public DateTime? FoundedDate { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }

        public List<CompanyReadViewNameHistoryDto> NameHistory { get; set; } = new();
        public List<CompanyReadViewEmailDto> Emails { get; set; } = new();
        public List<CompanyReadViewPhoneDto> Phones { get; set; } = new();
        public List<CompanyReadViewAddressDto> Addresses { get; set; } = new();
    }

    public class CompanyReadViewNameHistoryDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent => EndDate == null;
        public string? Description { get; set; }
        public List<CompanyReadViewNameHistoryTranslationDto> Translations { get; set; } = new();
    }

    public class CompanyReadViewNameHistoryTranslationDto
    {
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CompanyReadViewEmailDto
    {
        public Guid Id { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CompanyReadViewPhoneDto
    {
        public Guid Id { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public short CountryId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CompanyReadViewAddressDto
    {
        public Guid Id { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public short CountryId { get; set; }
        public short RegionId { get; set; }
        public int CityId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }
}
