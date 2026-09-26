namespace KSS.Service.IService
{
    /// <summary>
    /// The companies a caller may read: every company, or an explicit set.
    /// A caller may read a company when a live grant gives them Information level 1 or more on
    /// it, personally or through one of their roles. A live global role grant with that level
    /// covers every company. Inactive or deleted grants give nothing.
    /// </summary>
    public sealed record ReadableCompanies(bool All, IReadOnlySet<Guid> CompanyIds)
    {
        public static ReadableCompanies None { get; } = new(false, new HashSet<Guid>());

        public static ReadableCompanies Every { get; } = new(true, new HashSet<Guid>());

        public bool Includes(Guid companyId) => All || CompanyIds.Contains(companyId);
    }
}
