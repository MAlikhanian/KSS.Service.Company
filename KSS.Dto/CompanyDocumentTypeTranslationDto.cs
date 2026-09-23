namespace KSS.Dto
{
    public class CompanyDocumentTypeTranslationDto
    {
        public int CompanyDocumentTypeId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
