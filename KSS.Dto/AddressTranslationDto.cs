namespace KSS.Dto
{
    public class AddressTranslationDto
    {
        public Guid AddressId { get; set; }
        public short LanguageId { get; set; }
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
    }
}
