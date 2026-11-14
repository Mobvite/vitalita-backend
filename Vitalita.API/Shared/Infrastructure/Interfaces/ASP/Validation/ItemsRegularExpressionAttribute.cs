using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

/// <summary>
///     Validates that every string of a collection matches a regular expression.
/// </summary>
/// <remarks>
///     A <c>null</c> collection is valid (combine it with <c>[Required]</c>); a <c>null</c> item is not. The
///     <c>ErrorMessage</c> is a <c>.resx</c> key, localized by <c>LocalizedValidationAttributeAdapterProvider</c>.
/// </remarks>
/// <param name="pattern">The regular expression every item must match.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ItemsRegularExpressionAttribute(string pattern) : ValidationAttribute
{
    private readonly Regex _regex = new(pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    /// <summary>
    ///     Gets the regular expression every item must match.
    /// </summary>
    public string Pattern { get; } = pattern;

    /// <inheritdoc />
    public override bool IsValid(object? value) =>
        value is not IEnumerable items || items.Cast<object?>().All(item => item is string text && _regex.IsMatch(text));
}
