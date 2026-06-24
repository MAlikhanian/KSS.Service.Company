namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    public class NameHistoryInsertDto
    {
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public bool IsCurrent => EndDate == null;
    }
}
