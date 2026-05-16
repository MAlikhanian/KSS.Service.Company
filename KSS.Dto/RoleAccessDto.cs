namespace KSS.Dto
{
    public class RoleAccessDto
    {
        public Guid Id { get; set; }
        // NULL = grant applies to all companies.
        public Guid? CompanyId { get; set; }
        public Guid GrantedToRoleId { get; set; }
        public string Section { get; set; } = string.Empty;
        public int Level { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
