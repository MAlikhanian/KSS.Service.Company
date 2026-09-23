namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    // StorageInstanceId + file metadata are produced by the BFF (orchestrator upload) before this call.
    public class CompanyDocumentInsertDto
    {
        public Guid CompanyId { get; set; }
        public int CompanyDocumentTypeId { get; set; }
        public int StorageInstanceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
