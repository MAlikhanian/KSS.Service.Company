namespace KSS.Dto
{
    public class NameHistoryTranslationDto
    {
        public Guid NameHistoryId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
    }
}
