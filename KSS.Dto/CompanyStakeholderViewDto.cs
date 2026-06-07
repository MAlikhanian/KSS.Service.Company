namespace KSS.Dto
{
    public class CompanyStakeholderViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte RelatedPartyType { get; set; } // 1=Company, 2=Person
        public Guid RelatedPartyId { get; set; }
        public byte StakeholderTypeId { get; set; }
        public string StakeholderTypeName { get; set; } = string.Empty;
        public CompanyStakeholderHistoryViewDto? Current { get; set; }
        public List<CompanyStakeholderHistoryViewDto> History { get; set; } = new();
    }

    public class CompanyStakeholderHistoryViewDto
    {
        public Guid Id { get; set; }
        public decimal OwnershipPercentage { get; set; }
        public long ShareCount { get; set; }
        public Guid? BoardRepresentativePersonId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
