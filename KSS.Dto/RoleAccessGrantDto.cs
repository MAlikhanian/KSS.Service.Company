namespace KSS.Dto
{
    /// <summary>
    /// Payload from the role-access grant dialog: the caller sets levels for both
    /// sections in one shot. CompanyId == null means the grant applies to all
    /// companies (caller must be SuperAdmin/CompanyAdmin to do this).
    /// </summary>
    public class RoleAccessGrantDto
    {
        public Guid? CompanyId { get; set; }
        public Guid GrantedToRoleId { get; set; }
        public int InformationLevel { get; set; }
        public int AccessLevel { get; set; }
    }
}
