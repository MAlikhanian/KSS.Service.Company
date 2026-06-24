namespace KSS.Dto
{
    public class StakeholderHistoryInsertDto
    {
        // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
        public Guid CompanyStakeholderId { get; set; }
        public decimal OwnershipPercentage { get; set; }
        public long ShareCount { get; set; }
        public Guid? BoardRepresentativePersonId { get; set; }
        public DateTime RegistrationDate { get; set; } // Official legal filing date
        public DateTime EffectiveDate { get; set; } // When change takes effect
        public DateTime? EndDate { get; set; }
    }
}
