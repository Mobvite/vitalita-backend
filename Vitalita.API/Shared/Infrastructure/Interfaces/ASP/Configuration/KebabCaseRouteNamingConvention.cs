using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     MVC controller convention that rewrites the <c>[controller]</c> route token to kebab-case
///     (<c>OlderAdultsController</c> → <c>api/v1/older-adults</c>).
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    /// <summary>
    ///     Applies the kebab-case transformation to the controller selectors and to its action selectors.
    /// </summary>
    /// <param name="controller">The controller model to transform.</param>
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

        foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel != null
            ? new AttributeRouteModel(selector.AttributeRouteModel) // keeps route Name and Order
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            }
            : null;
    }
}
