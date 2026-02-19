namespace KSS.Dto
{
    public class CompanyNameHistoryTranslationDto
    {
        public Guid CompanyNameHistoryId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
    }
}
