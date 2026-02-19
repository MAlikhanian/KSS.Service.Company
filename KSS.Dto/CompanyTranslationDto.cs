namespace KSS.Dto
{
    public class CompanyTranslationDto
    {
        public Guid CompanyId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
}
