namespace KSS.Dto
{
    /// <summary>
    /// DTO for upserting translations on an existing name history entry.
    /// Used by CompanyNameManagementService.
    /// </summary>
    public class UpsertNameTranslationsDto
    {
        public Guid NameHistoryId { get; set; }
        public Guid CompanyId { get; set; }
        public List<CompanyNameHistoryTranslationDto> Translations { get; set; } = new();
    }
}
