namespace KSS.Dto
{
    /// <summary>
    /// Company-name translation payload for an INSERT. Carries no parent FK GUID —
    /// the backend assigns the company id (v7) and links the translation.
    /// </summary>
    public class CompanyTranslationInsertDto
    {
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Name-history translation payload for an INSERT. Carries no parent FK GUID —
    /// the backend assigns the name-history id (v7) and links the translation.
    /// </summary>
    public class NameHistoryTranslationInsertDto
    {
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
    }

    public class CompanyNameHistoryItemDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<NameHistoryTranslationInsertDto> Translations { get; set; } = new();
    }

    /// <summary>
    /// INSERT DTO for creating a company. Carries NO backend-generated GUIDs:
    /// the company id and all child ids (name-history, translations) are assigned
    /// in the backend as v7. References to existing rows (legal form, industry,
    /// geography) are scalar lookup ids, not GUIDs.
    /// </summary>
    public class CompanyInsertDto
    {
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
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Company Translations
        public List<CompanyTranslationInsertDto> Translations { get; set; } = new();

        // Name History (typically one entry for initial name)
        public CompanyNameHistoryItemDto? NameHistory { get; set; }
    }
}
