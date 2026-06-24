namespace KSS.Dto
{
    public class CompanyStakeholderViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte RelatedPartyType { get; set; } // 1=Company, 2=Person
        public Guid RelatedPartyId { get; set; }
        // Resolved display name of the related party (company name for type 1,
        // person name for type 2). Filled by the backend so the frontend does
        // not join against a person/company directory client-side.
        public string RelatedPartyName { get; set; } = string.Empty;
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
        // Resolved display name of the board representative person (null when
        // there is no board rep). Filled by the backend.
        public string? BoardRepresentativeName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
