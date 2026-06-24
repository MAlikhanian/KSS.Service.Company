namespace KSS.Dto
{
    /// <summary>
    /// INSERT DTO for adding a new company name-history entry with all its
    /// translations in one call. Carries NO backend-generated GUID — the
    /// name-history id is assigned in the backend (v7). CompanyId is a reference
    /// to the EXISTING company the name belongs to (input, in the body).
    /// Used by CompanyNameManagementService.
    /// </summary>
    public class AddNameWithTranslationsDto
    {
        public Guid CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public List<NameHistoryTranslationInsertDto> Translations { get; set; } = new();
    }
}
