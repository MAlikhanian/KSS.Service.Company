namespace KSS.Dto
{
    public class StakeholderInsertDto
    {
        // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
        public Guid CompanyId { get; set; }
        public byte RelatedPartyType { get; set; } // 1=Company, 2=Person
        public Guid RelatedPartyId { get; set; }
        public byte StakeholderTypeId { get; set; }
    }
}
