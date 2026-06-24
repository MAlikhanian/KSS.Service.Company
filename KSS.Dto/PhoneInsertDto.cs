namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    public class PhoneInsertDto
    {
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public short CountryId { get; set; } // Phone number's country code context (separate from Company.RegistrationCountryId)
        public string PhoneNumber { get; set; } = string.Empty; // E.164 format: + and 7-15 digits
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
