namespace KSS.Dto
{
    public class CompanyStakeholderHistoryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyStakeholderId { get; set; }
        public decimal OwnershipPercentage { get; set; }
        public long ShareCount { get; set; }
        public Guid? BoardRepresentativePersonId { get; set; }
        public DateTime RegistrationDate { get; set; } // Official legal filing date
        public DateTime EffectiveDate { get; set; } // When change takes effect
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
