using Humanizer;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     String extensions used by the database naming conventions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Converts a PascalCase or camelCase string to snake_case using Humanizer.
    /// </summary>
    /// <remarks>
    ///     Acronyms stay together: <c>IX_OlderAdults</c> → <c>ix_older_adults</c>,
    ///     <c>PK_Users</c> → <c>pk_users</c>.
    /// </remarks>
    /// <param name="text">The string to convert.</param>
    /// <returns>The snake_case representation of <paramref name="text" />.</returns>
    public static string ToSnakeCase(this string text)
    {
        return string.IsNullOrEmpty(text) ? text : text.Underscore();
    }

    /// <summary>
    ///     Pluralizes an English word using Humanizer.
    /// </summary>
    /// <param name="text">The word to pluralize.</param>
    /// <returns>The plural form of <paramref name="text" />.</returns>
    public static string ToPlural(this string text)
    {
        return text.Pluralize(false);
    }
}
