namespace KSS.Dto
{
    /// <summary>
    /// All contact data for a company: emails, phones, addresses.
    /// Includes label names for display.
    /// </summary>
    public class CompanyContactDto
    {
        public List<CompanyEmailViewDto> Emails { get; set; } = new();
        public List<CompanyPhoneViewDto> Phones { get; set; } = new();
        public List<CompanyAddressViewDto> Addresses { get; set; } = new();
    }

    public class CompanyEmailViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CompanyPhoneViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public short CountryId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CompanyAddressViewDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public short CountryId { get; set; }
        public short RegionId { get; set; }
        public int CityId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }
}
