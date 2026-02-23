namespace KSS.Dto
{
    /// <summary>
    /// DTO for company general-information form.
    /// Flattens Company + CompanyTranslation + CompanyNameHistory into a single object.
    /// Fields that don't exist yet in the database are left for future implementation.
    /// </summary>
    public class CompanyDetailDto
    {
        public Guid Id { get; set; }

        // From CompanyTranslation (LanguageId = 12 Persian)
        public string CompanyPersianName { get; set; } = string.Empty;

        // From CompanyTranslation (LanguageId = 10 English)
        public string? CompanyLatinName { get; set; }

        // From CompanyNameHistory (past entries concatenated)
        public string? FormerNames { get; set; }

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

        // Name history entries
        public List<CompanyNameHistoryDto> NameHistory { get; set; } = new();
    }
}
