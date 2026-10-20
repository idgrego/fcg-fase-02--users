using System.Text.RegularExpressions;

namespace FCG02.UsersAPI.Domain.ValueObjects
{
    public record Email
    {
        private const int MaxLength = 256;
        public string Value { get; private set; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty.", nameof(value));

            var sanitized = value.Trim().ToLowerInvariant();

            if (sanitized.Length > MaxLength)
                throw new ArgumentException($"Email cannot exceed {MaxLength} characters.", nameof(value));

            if (!EmailRegex.IsMatch(sanitized))
                throw new ArgumentException("Invalid email format.", nameof(value));

            Value = sanitized;
        }

        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;

        public static explicit operator Email(string value) => new Email(value);

        private static Regex EmailRegex;
        static Email()
        {
            EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}