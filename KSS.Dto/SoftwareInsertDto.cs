namespace KSS.Dto
{
    // No Id — the backend/database stamps the identity value.
    public class SoftwareInsertDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
    }
}
