namespace KSS.Helper
{
    public static class WebsiteHelper
    {
        /// <summary>
        /// Normalizes a website URL: trims whitespace and validates against the
        /// DB CK_Website_Url guard (no spaces, length >= 3).
        /// </summary>
        public static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Website URL cannot be empty.", nameof(url));

            var normalized = url.Trim();

            if (normalized.Contains(' ') || normalized.Length < 3)
                throw new ArgumentException("Invalid website URL.", nameof(url));

            return normalized;
        }
    }
}
