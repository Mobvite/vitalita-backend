using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     Localizes the <c>ErrorMessage</c> of every DataAnnotations attribute, including the ones that ASP.NET Core
///     does not localize by itself (<c>AllowedValues</c>, <c>DeniedValues</c>, <c>Length</c>, <c>Base64String</c>, ...).
/// </summary>
/// <remarks>
///     MVC only localizes attributes that have an adapter in the default provider (<c>Required</c>,
///     <c>StringLength</c>, <c>RegularExpression</c>, ...). For any other attribute it returns the raw
///     <c>ErrorMessage</c>, so the response would contain the <c>.resx</c> key. This provider keeps the default
///     adapters and adds a generic one for the rest.
/// </remarks>
public class LocalizedValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider _defaultProvider = new();

    /// <inheritdoc />
    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer) =>
        _defaultProvider.GetAttributeAdapter(attribute, stringLocalizer) ??
        new LocalizedAttributeAdapter(attribute, stringLocalizer);

    /// <summary>
    ///     Generic adapter that only localizes the error message (no client-side validation).
    /// </summary>
    private sealed class LocalizedAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
        : AttributeAdapterBase<ValidationAttribute>(attribute, stringLocalizer)
    {
        public override void AddValidation(ClientModelValidationContext context)
        {
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext) =>
            GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
    }
}
