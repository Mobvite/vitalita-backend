using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Interfaces.REST.Resources;

namespace Vitalita.API.Shared.Interfaces.REST.Transform;

/// <summary>
///     Localized OpenAPI metadata of the health endpoint.
/// </summary>
public static class HealthOpenApiAssembler
{
    private const string ControllerName = "Health";

    /// <summary>
    ///     Localizes the summary, description and 200 response of <c>GET /health</c>.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being transformed.</param>
    /// <param name="context">The transformer context, used to resolve the localizer and action descriptor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor { ControllerName: ControllerName } action)
            return Task.CompletedTask;

        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        operation.OperationId = action.ActionName;
        operation.Summary = localizer["HealthGetHealthSummary"].Value;
        operation.Description = localizer["HealthGetHealthDescription"].Value;
        if (operation.Responses?.TryGetValue("200", out var response) == true && response is OpenApiResponse ok)
            ok.Description = localizer["HealthOkResponse"].Value;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Localizes the description of <see cref="HealthResource" /> and its <c>status</c> property.
    /// </summary>
    /// <param name="schema">The OpenAPI schema being transformed.</param>
    /// <param name="context">The transformer context, used to resolve the localizer and type information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformSchemaAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        if (context.JsonTypeInfo.Type == typeof(HealthResource))
            schema.Description = localizer["HealthResourceSchema"].Value;
        else if (context.JsonPropertyInfo?.DeclaringType == typeof(HealthResource))
            schema.Description = localizer["HealthStatusProperty"].Value;
        return Task.CompletedTask;
    }
}
