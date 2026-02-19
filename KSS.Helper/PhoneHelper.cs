using System.Text.RegularExpressions;

namespace KSS.Helper
{
    public static class PhoneHelper
    {
        /// <summary>
        /// Validates phone number in E.164 format: + followed by 7-15 digits, total length 8-16.
        /// </summary>
        public static void ValidateE164(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

            // E.164 validation: + followed by 7-15 digits, total length 8-16
            if (!phoneNumber.StartsWith('+'))
                throw new ArgumentException("Phone number must start with '+' (E.164 format).", nameof(phoneNumber));

            if (phoneNumber.Length < 8 || phoneNumber.Length > 16)
                throw new ArgumentException("Phone number length must be between 8 and 16 characters (E.164: + and 7-15 digits).", nameof(phoneNumber));

            var digitsPart = phoneNumber.Substring(1);
            if (digitsPart.Length < 7 || digitsPart.Length > 15)
                throw new ArgumentException("Phone number must have 7-15 digits after '+' (E.164 format).", nameof(phoneNumber));

            if (!Regex.IsMatch(digitsPart, @"^\d+$"))
                throw new ArgumentException("Phone number must contain only digits after '+' (E.164 format).", nameof(phoneNumber));

            if (phoneNumber.IndexOf('+', 1) != -1)
                throw new ArgumentException("Phone number must contain only one '+' at the beginning.", nameof(phoneNumber));
        }
    }
}
