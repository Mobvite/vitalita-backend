using System.Text.RegularExpressions;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
///     String extensions used by the ASP.NET Core infrastructure.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     Converts a PascalCase or camelCase string to kebab-case.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The kebab-case representation of <paramref name="text" /> (<c>OlderAdults</c> → <c>older-adults</c>).</returns>
    public static string ToKebabCase(this string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        return KebabCaseRegex().Replace(text, "-$1")
            .Trim()
            .ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])")]
    private static partial Regex KebabCaseRegex();
}
