namespace KSS.Dto
{
    public class PhoneLabelTranslationDto
    {
        public byte PhoneLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
