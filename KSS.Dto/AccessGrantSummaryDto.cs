namespace KSS.Dto
{
    /// <summary>
    /// One entry per (CompanyId, GrantedToPersonId) pair, with both section
    /// levels rolled up. Powers the grant table on the access management page.
    /// </summary>
    public class AccessGrantSummaryDto
    {
        public Guid CompanyId { get; set; }
        public Guid GrantedToPersonId { get; set; }
        public int InformationLevel { get; set; }
        public int AccessLevel { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
