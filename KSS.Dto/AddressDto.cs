namespace KSS.Dto
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public short CountryId { get; set; }
        public short RegionId { get; set; }
        public int CityId { get; set; }
        public string PostalCode { get; set; } = string.Empty; // Multi-country; some codes exceed 10 or include letters/spaces
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
