using System.Text.RegularExpressions;

namespace KSS.Helper
{
    public static class EmailHelper
    {
        /// <summary>
        /// Normalizes email address: trims whitespace and converts to lowercase.
        /// Matches database collation Latin1_General_100_CI_AS behavior.
        /// </summary>
        public static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address cannot be empty.", nameof(email));

            // Trim and lowercase (matches database collation Latin1_General_100_CI_AS)
            var normalized = email.Trim().ToLowerInvariant();

            // Basic format validation (matches CK_Email_Format)
            if (!Regex.IsMatch(normalized, @"^.+@.+\..+$") || normalized.Contains(' '))
                throw new ArgumentException("Invalid email format.", nameof(email));

            return normalized;
        }
    }
}
