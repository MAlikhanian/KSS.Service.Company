namespace KSS.Dto
{
    public class AccessDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GrantedToPersonId { get; set; }
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
