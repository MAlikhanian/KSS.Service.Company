namespace KSS.Dto
{
    public class WebsiteLabelTranslationDto
    {
        public byte WebsiteLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
