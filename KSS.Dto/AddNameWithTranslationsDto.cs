namespace KSS.Dto
{
    /// <summary>
    /// DTO for adding a new company name history entry with all its translations in one call.
    /// Used by CompanyNameManagementService.
    /// </summary>
    public class AddNameWithTranslationsDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public List<CompanyNameHistoryTranslationDto> Translations { get; set; } = new();
    }
}
