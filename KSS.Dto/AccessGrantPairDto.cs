namespace KSS.Dto
{
    /// <summary>
    /// Minimal (CompanyId, GrantedToPersonId) pair used by cross-service
    /// dashboard report endpoints. Exposed by /Api/Access/ListAllGrants for
    /// the Person service to enumerate access grants without pulling the full
    /// AccessGrantSummaryDto (which carries section levels + timestamps).
    /// </summary>
    public class AccessGrantPairDto
    {
        public System.Guid CompanyId { get; set; }
        public System.Guid GrantedToPersonId { get; set; }
    }
}
