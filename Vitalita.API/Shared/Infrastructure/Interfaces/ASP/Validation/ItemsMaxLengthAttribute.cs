using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

/// <summary>
///     Validates that every string of a collection has at most <see cref="MaxLength" /> characters.
/// </summary>
/// <remarks>
///     Use it with <c>[MaxLength]</c> for the number of items. A <c>null</c> collection or <c>null</c> items are
///     valid. The <c>ErrorMessage</c> is a <c>.resx</c> key, localized by <c>LocalizedValidationAttributeAdapterProvider</c>.
/// </remarks>
/// <param name="maxLength">Maximum length of each item.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ItemsMaxLengthAttribute(int maxLength) : ValidationAttribute
{
    /// <summary>
    ///     Gets the maximum length of each item.
    /// </summary>
    public int MaxLength { get; } = maxLength;

    /// <inheritdoc />
    public override bool IsValid(object? value) =>
        value is not IEnumerable items || items.OfType<string>().All(item => item.Trim().Length <= MaxLength);
}
