namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    public class WebsiteInsertDto
    {
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string Url { get; set; } = string.Empty; // Will be normalized (trimmed) before save
        public bool IsPrimary { get; set; }
    }
}
