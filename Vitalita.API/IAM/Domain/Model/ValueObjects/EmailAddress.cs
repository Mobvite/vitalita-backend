using System.Text.RegularExpressions;

namespace Vitalita.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Value object for an account email address.
/// </summary>
/// <remarks>
///     The value is trimmed and stored in lower case, so lookups and the unique index are case-insensitive.
///     The format rule is the same one used by the frontend <c>Email</c> value object.
/// </remarks>
public sealed partial record EmailAddress
{
    /// <summary>
    ///     Maximum length of an email address (RFC 5321).
    /// </summary>
    public const int MaxLength = 254;

    /// <summary>
    ///     Format accepted for an email address (same rule as the frontend).
    /// </summary>
    public const string Pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$";

    /// <summary>
    ///     Initializes a new <see cref="EmailAddress" /> with a normalized value.
    /// </summary>
    /// <param name="value">The email typed by the user.</param>
    /// <exception cref="ArgumentException">Thrown when the value is empty, too long or not a valid email.</exception>
    public EmailAddress(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length == 0)
            throw new ArgumentException("Email cannot be empty.", nameof(value));
        if (normalized.Length > MaxLength)
            throw new ArgumentException($"Email cannot be longer than {MaxLength} characters.", nameof(value));
        if (!EmailRegex().IsMatch(normalized))
            throw new ArgumentException("Email format is not valid.", nameof(value));

        Value = normalized;
    }

    /// <summary>
    ///     Gets the normalized email.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Checks whether a value can be used to create an <see cref="EmailAddress" />.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> when the value is a valid email.</returns>
    public static bool IsValid(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return normalized.Length is > 0 and <= MaxLength && EmailRegex().IsMatch(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    [GeneratedRegex(Pattern)]
    private static partial Regex EmailRegex();
}
