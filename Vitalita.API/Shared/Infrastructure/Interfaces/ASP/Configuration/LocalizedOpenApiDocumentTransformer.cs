using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.Resources;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     Sets the localized title and description of the OpenAPI document.
/// </summary>
/// <remarks>
///     Operation and schema descriptions are localized by each bounded context with its own assembler.
/// </remarks>
public static class LocalizedOpenApiDocumentTransformer
{
    /// <summary>
    ///     Applies the localized document information.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="context">The transformer context, used to resolve the localizer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        document.Info = new OpenApiInfo
        {
            Title = localizer["ApiTitle"].Value,
            Description = localizer["ApiDescription"].Value,
            Version = "v1"
        };
        return Task.CompletedTask;
    }
}
