namespace KSS.Dto
{
    public class CompanyStakeholderDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte RelatedPartyType { get; set; } // 1=Company, 2=Person
        public Guid RelatedPartyId { get; set; }
        public byte StakeholderTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
