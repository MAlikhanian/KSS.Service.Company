namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    public class EmailInsertDto
    {
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string EmailAddress { get; set; } = string.Empty; // Will be normalized (trimmed, lowercase) before save
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
