namespace KSS.Dto
{
    public class EmailDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string EmailAddress { get; set; } = string.Empty; // Will be normalized (trimmed, lowercase) before save
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
