namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Normalizes the free text received by the Profiles aggregates.
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    ///     Trims a value and turns <c>null</c> into an empty string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The trimmed value.</returns>
    public static string Clean(string? value) => value?.Trim() ?? string.Empty;

    /// <summary>
    ///     Trims every item, removes the empty ones and the case-insensitive duplicates, keeping the order.
    /// </summary>
    /// <param name="values">The values (may be <c>null</c>).</param>
    /// <returns>The normalized list.</returns>
    public static IReadOnlyList<string> CleanList(IEnumerable<string?>? values) =>
        (values ?? [])
        .Select(Clean)
        .Where(value => value.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    /// <summary>
    ///     Normalizes an email: trimmed and lowercase.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <returns>The normalized email.</returns>
    public static string NormalizeEmail(string? email) => Clean(email).ToLowerInvariant();
}
