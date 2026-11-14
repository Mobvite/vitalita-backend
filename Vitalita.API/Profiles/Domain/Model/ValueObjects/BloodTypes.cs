namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Blood types accepted for an older adult. An empty string means "unknown".
/// </summary>
public static class BloodTypes
{
    /// <summary>The accepted values, including the empty string.</summary>
    public static readonly IReadOnlyList<string> Allowed = ["", "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-"];

    /// <summary>
    ///     Checks whether a value is an accepted blood type.
    /// </summary>
    /// <param name="value">The value to check (<c>null</c> counts as empty).</param>
    /// <returns><c>true</c> when the value is empty or one of the eight blood types.</returns>
    public static bool IsValid(string? value) => Allowed.Contains((value ?? string.Empty).Trim().ToUpperInvariant());
}
