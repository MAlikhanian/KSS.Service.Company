namespace KSS.Service.Client
{
    /// <summary>
    /// Typed client for KSS.Service.Person. Resolves person display names for a
    /// specific set of ids (stakeholder related parties + board representatives)
    /// so the Company backend can return ready-to-display names instead of the
    /// frontend joining against a person directory.
    /// </summary>
    public interface IPersonApiClient
    {
        /// <summary>
        /// Resolve display names for a set of person ids via POST /Api/Person/Names.
        /// Returns a map personId -> display name ("First Last", falling back to
        /// NationalId). Degrades to an EMPTY map if the Person service is
        /// unreachable or errors — never throws (so a Person outage cannot turn
        /// the stakeholder list into an HTTP 500).
        /// </summary>
        Task<IReadOnlyDictionary<Guid, string>> GetPersonNamesAsync(
            IReadOnlyCollection<Guid> personIds,
            short languageId = 12,
            CancellationToken cancellationToken = default);
    }
}
