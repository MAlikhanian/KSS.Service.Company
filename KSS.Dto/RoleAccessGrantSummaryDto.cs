namespace KSS.Dto
{
    /// <summary>
    /// One entry per (CompanyId, GrantedToRoleId) pair, with both section
    /// levels rolled up. CompanyId is null for global grants (applies to all).
    /// </summary>
    public class RoleAccessGrantSummaryDto
    {
        public Guid? CompanyId { get; set; }
        public Guid GrantedToRoleId { get; set; }
        public int InformationLevel { get; set; }
        public int AccessLevel { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
