namespace KSS.Dto
{
    public class AddressLabelTranslationDto
    {
        public byte AddressLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
