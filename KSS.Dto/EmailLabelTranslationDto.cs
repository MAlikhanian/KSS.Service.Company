namespace KSS.Dto
{
    public class EmailLabelTranslationDto
    {
        public byte EmailLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
