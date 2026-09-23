namespace KSS.Dto
{
    public class CompanyOwnershipViewDto
    {
        public Guid Id { get; set; }
        public Guid OwnerCompanyId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
