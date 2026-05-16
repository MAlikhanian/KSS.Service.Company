namespace KSS.Dto
{
    /// <summary>
    /// Single-row add DTO. Used internally where a single (CompanyId,
    /// GrantedToPersonId, Section, Level) row needs to be created.
    /// External callers should prefer <see cref="AccessGrantDto"/>, which
    /// carries both section levels in one payload.
    /// </summary>
    public class AccessAddDto
    {
        public Guid CompanyId { get; set; }
        public Guid GrantedToPersonId { get; set; }
        public string Section { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}
