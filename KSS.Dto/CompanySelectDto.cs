namespace KSS.Dto
{
    /// <summary>
    /// Lightweight DTO for company select dropdowns.
    /// Returns company with all name history entries.
    /// </summary>
    public class CompanySelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? NationalId { get; set; }
        public string? Website { get; set; }
        public List<CompanyNameHistoryDto> NameHistory { get; set; } = new();
    }
}
