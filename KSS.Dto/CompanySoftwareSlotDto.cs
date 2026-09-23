namespace KSS.Dto
{
    /// <summary>One category slot for a company: the category + the chosen software (or null).</summary>
    public class CompanySoftwareSlotDto
    {
        public byte SoftwareCategoryId { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; // translated
        public int? SoftwareId { get; set; }                     // null = not set
        public string? SoftwareName { get; set; }
        public Guid? ProviderCompanyId { get; set; }
        public string? ProviderCompanyName { get; set; }
    }

    public class CompanySoftwareUpsertDto
    {
        public byte SoftwareCategoryId { get; set; }
        public int SoftwareId { get; set; }
    }

    /// <summary>One active software product + its provider company (for the cascade dropdowns).</summary>
    public class SoftwareCatalogItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty; // provider, translated
    }
}
