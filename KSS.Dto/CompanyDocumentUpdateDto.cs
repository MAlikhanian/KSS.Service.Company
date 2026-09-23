namespace KSS.Dto
{
    // Id + only the editable fields. StorageInstanceId/FileSize/ContentType and
    // audit fields are never edited here (the blob replace is a separate BFF step;
    // FileName is refreshed only when a new file is uploaded).
    public class CompanyDocumentUpdateDto
    {
        public Guid Id { get; set; }
        public int CompanyDocumentTypeId { get; set; }
        // Nullable: sent only when a new file is uploaded. Null/blank ⇒ keep the
        // existing stored name (see CompanyDocumentService.UpdateDto). Nullable so
        // an omitted value doesn't trip [ApiController] implicit-required.
        public string? FileName { get; set; }
    }
}
