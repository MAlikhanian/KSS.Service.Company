namespace KSS.Dto
{
    /// <summary>
    /// Payload from the frontend grant dialog: the caller sets levels for both
    /// sections in one shot. Sections with Level == 0 are skipped (no row
    /// inserted) — absence of a row means "no access".
    /// </summary>
    public class AccessGrantDto
    {
        public Guid CompanyId { get; set; }
        public Guid GrantedToPersonId { get; set; }
        public int InformationLevel { get; set; }
        public int AccessLevel { get; set; }
    }
}
