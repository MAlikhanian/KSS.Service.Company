namespace KSS.Dto
{
    /// <summary>
    /// Lightweight DTO for brokerage/company select dropdowns.
    /// Returns company with its translated name in a single object.
    /// </summary>
    public class BrokerageSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? NationalId { get; set; }
        public string? Website { get; set; }
    }
}
