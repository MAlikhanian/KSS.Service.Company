namespace KSS.Dto
{
    /// <summary>
    /// Minimal payload for DELETE /Api/CompanyNameManagement/DeleteNameHistory.
    /// HTTP endpoints bind DTOs, not entities — entities carry navigation
    /// properties (e.g. NameHistory.Company) that ASP.NET's [ApiController]
    /// validator would otherwise demand on the wire.
    /// </summary>
    public class NameHistoryDeleteDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
    }
}
