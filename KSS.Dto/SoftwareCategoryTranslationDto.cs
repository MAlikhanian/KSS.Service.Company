namespace KSS.Dto
{
    public class SoftwareCategoryTranslationDto
    {
        public byte SoftwareCategoryId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
