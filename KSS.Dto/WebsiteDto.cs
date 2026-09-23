namespace KSS.Dto
{
    public class WebsiteDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string Url { get; set; } = string.Empty; // Will be normalized (trimmed) before save
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
