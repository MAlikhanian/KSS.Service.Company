namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID; a client GUID cannot bind.
    public class CompanySoftwareInsertDto
    {
        public Guid CompanyId { get; set; }
        public byte SoftwareCategoryId { get; set; }
        public int SoftwareId { get; set; }
    }
}
