namespace KSS.Service.Service
{
    /// <summary>
    /// Chooses the display name for a company from its translations. Order: the requested
    /// language, then Persian, then English, then the lowest remaining language id. Blank
    /// names are skipped. When no language has a name, the supplied last resort is returned.
    /// </summary>
    public static class CompanyDisplayName
    {
        private const short Persian = 12;
        private const short English = 10;

        public static string PickCompanyDisplayName(
            IEnumerable<(short LanguageId, string? Name)> translations,
            short requestedLanguageId,
            string lastResort)
        {
            var chosen = translations
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .OrderBy(t => t.LanguageId == requestedLanguageId ? 0
                            : t.LanguageId == Persian ? 1
                            : t.LanguageId == English ? 2
                            : 3)
                .ThenBy(t => t.LanguageId)
                .ThenBy(t => t.Name, StringComparer.Ordinal)
                .Select(t => t.Name)
                .FirstOrDefault();

            return chosen ?? lastResort;
        }
    }
}
