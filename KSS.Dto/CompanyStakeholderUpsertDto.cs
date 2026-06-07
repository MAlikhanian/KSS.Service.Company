namespace KSS.Dto
{
    public class CompanyStakeholderUpsertDto
    {
        public byte RelatedPartyType { get; set; } // 1=Company, 2=Person
        public Guid RelatedPartyId { get; set; }
        public byte StakeholderTypeId { get; set; }

        // Initial / new current history fields. On PUT these are compared to the
        // current row — if any history-shaped field changed, the previous current
        // row is closed (EndDate = today) and a new current row is appended.
        public decimal OwnershipPercentage { get; set; }
        public long ShareCount { get; set; }
        public Guid? BoardRepresentativePersonId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
