namespace KSS.Dto
{
    public class PhoneDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public short CountryId { get; set; } // Phone number's country code context (separate from Company.RegistrationCountryId)
        public string PhoneNumber { get; set; } = string.Empty; // E.164 format: + and 7-15 digits
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
