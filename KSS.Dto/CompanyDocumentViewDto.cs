namespace KSS.Dto
{
    public class CompanyDocumentViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public int CompanyDocumentTypeId { get; set; }
        public int StorageInstanceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
