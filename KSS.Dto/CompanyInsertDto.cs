namespace KSS.Dto
{
    public class CompanyNameHistoryItemDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CompanyNameHistoryTranslationDto> Translations { get; set; } = new List<CompanyNameHistoryTranslationDto>();
    }

    public class CompanyInsertDto
    {
        // Company fields
        public Guid Id { get; set; }
        public byte CompanyTypeId { get; set; }
        public short? IndustryId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string EconomicCode { get; set; } = string.Empty;
        public short RegistrationCountryId { get; set; }
        public short RegistrationRegionId { get; set; }
        public int RegistrationCityId { get; set; }
        public string? TaxId { get; set; }
        public DateTime? FoundedDate { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Company Translations
        public List<CompanyTranslationDto> Translations { get; set; } = new List<CompanyTranslationDto>();

        // Name History (typically one entry for initial name)
        public CompanyNameHistoryItemDto? NameHistory { get; set; }
    }
}
