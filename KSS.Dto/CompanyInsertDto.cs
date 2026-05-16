namespace KSS.Dto
{
    public class CompanyNameHistoryItemDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<NameHistoryTranslationDto> Translations { get; set; } = new List<NameHistoryTranslationDto>();
    }

    public class CompanyInsertDto
    {
        // Company fields
        public Guid Id { get; set; }
        public byte LegalFormId { get; set; }
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
        public List<TranslationDto> Translations { get; set; } = new List<TranslationDto>();

        // Name History (typically one entry for initial name)
        public CompanyNameHistoryItemDto? NameHistory { get; set; }
    }
}
