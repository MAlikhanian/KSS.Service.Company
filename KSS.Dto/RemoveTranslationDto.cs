namespace KSS.Dto
{
    /// <summary>
    /// DTO for removing a single translation from a name history entry.
    /// Used by CompanyNameManagementService.
    /// </summary>
    public class RemoveTranslationDto
    {
        public Guid NameHistoryId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
