namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Code that a family member uses to accept an invitation, for example <c>VITA-7KQ2M9XH</c>.
/// </summary>
/// <remarks>
///     New codes have the <see cref="Prefix" /> and <see cref="RandomLength" /> characters of <see cref="Alphabet" />
///     (no 0/O or 1/I, so they are easy to read). Codes are compared in uppercase. Shorter codes created before this
///     format (<c>VITA-XXXX</c>) are still valid values.
/// </remarks>
public record InvitationCode
{
    /// <summary>Prefix of every new code.</summary>
    public const string Prefix = "VITA-";

    /// <summary>Characters used in the random part (same alphabet as the frontend).</summary>
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>Number of random characters of a new code (32^8, about 40 bits).</summary>
    public const int RandomLength = 8;

    /// <summary>Maximum length of a stored code.</summary>
    public const int MaxLength = 20;

    /// <summary>
    ///     Creates a code from a value received from the user or the database.
    /// </summary>
    /// <param name="value">The code; it is trimmed and converted to uppercase.</param>
    /// <exception cref="ArgumentException">Thrown when the value is empty or longer than <see cref="MaxLength" />.</exception>
    public InvitationCode(string value)
    {
        var normalized = Normalize(value);
        if (normalized.Length == 0 || normalized.Length > MaxLength)
            throw new ArgumentException($"An invitation code has 1 to {MaxLength} characters.", nameof(value));
        Value = normalized;
    }

    /// <summary>
    ///     Gets the normalized code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Trims a code and converts it to uppercase.
    /// </summary>
    /// <param name="value">The raw code.</param>
    /// <returns>The normalized code (empty for <c>null</c>).</returns>
    public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    ///     Checks whether a raw value can be an invitation code.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <returns><c>true</c> when it is not empty and not longer than <see cref="MaxLength" /> once normalized.</returns>
    public static bool IsValid(string? value) => Normalize(value) is { Length: > 0 and <= MaxLength };

    /// <summary>
    ///     Checks whether a code has the format of the codes created by the server.
    /// </summary>
    /// <param name="value">The code.</param>
    /// <returns><c>true</c> for <c>VITA-</c> followed by 8 characters of <see cref="Alphabet" />.</returns>
    public static bool HasGeneratedFormat(string value) =>
        value.Length == Prefix.Length + RandomLength &&
        value.StartsWith(Prefix, StringComparison.Ordinal) &&
        value[Prefix.Length..].All(Alphabet.Contains);

    /// <inheritdoc />
    public override string ToString() => Value;
}
