namespace KSS.Dto
{
    /// <summary>
    /// The signed-in caller's own access grants, and the companies those grants name.
    /// </summary>
    public class MyGrantsDto
    {
        public List<MyGrantDto> Grants { get; set; } = new();

        /// <summary>One entry per distinct company named by a grant. Grants that cover every company add none.</summary>
        public List<MyGrantCompanyDto> Companies { get; set; } = new();
    }

    /// <summary>
    /// One grant row of the caller: a personal grant, or a grant held through one of the caller's roles.
    /// Rows are returned live or not; the flags say which.
    /// </summary>
    public class MyGrantDto
    {
        /// <summary>The company the grant is on; null for a grant that covers every company.</summary>
        public Guid? CompanyId { get; set; }

        public byte SectionId { get; set; }

        public int Level { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class MyGrantCompanyDto
    {
        public Guid CompanyId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>The company's name in each language: every translation that is not deleted. Empty when it has none.</summary>
        public List<CompanyNameDto> Names { get; set; } = new();
    }

    public class CompanyNameDto
    {
        public short LanguageId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ShortName { get; set; }
    }
}
