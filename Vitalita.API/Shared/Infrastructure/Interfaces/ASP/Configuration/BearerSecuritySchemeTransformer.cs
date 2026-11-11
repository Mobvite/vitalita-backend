using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     Documents the JWT Bearer security scheme in the OpenAPI document.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>
///             <description>
///                 Adds <c>components.securitySchemes.Bearer</c> (<c>http</c>, <c>bearer</c>, <c>JWT</c>), so Swagger UI
///                 shows the Authorize button.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Adds <c>security: [{ Bearer: [] }]</c> to operations that require authentication
///                 (<c>[Authorize]</c> without <c>[AllowAnonymous]</c>), so Swagger UI shows the lock icon and sends
///                 the token.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Operations marked with <see cref="OptionalBearerAuthenticationAttribute" /> get
///                 <c>security: [{}, { Bearer: [] }]</c> (token optional).
///             </description>
///         </item>
///     </list>
/// </remarks>
public static class BearerSecuritySchemeTransformer
{
    /// <summary>
    ///     Name of the security scheme in <c>components.securitySchemes</c>.
    /// </summary>
    public const string SchemeName = "Bearer";

    /// <summary>
    ///     OpenAPI document transformer entry point.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="context">The transformer context, used to resolve the localizer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformDocumentAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        ApplyToDocument(document, localizer["BearerSchemeDescription"].Value);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     OpenAPI operation transformer entry point.
    /// </summary>
    /// <param name="operation">The operation being transformed.</param>
    /// <param name="context">The transformer context, used to read the endpoint metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ApplyToOperation(operation, context.Description.ActionDescriptor.EndpointMetadata, context.Document);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Adds the Bearer security scheme to the document components.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="description">The localized description of the scheme.</param>
    public static void ApplyToDocument(OpenApiDocument document, string description)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = description
        };
    }

    /// <summary>
    ///     Adds the security requirement to an operation according to its authorization metadata.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="endpointMetadata">The endpoint metadata (controller and action attributes).</param>
    /// <param name="document">The document that owns the scheme, used to resolve the reference.</param>
    public static void ApplyToOperation(OpenApiOperation operation, IEnumerable<object> endpointMetadata,
        OpenApiDocument? document)
    {
        var metadata = endpointMetadata.ToList();
        var bearerRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
        };

        if (metadata.OfType<OptionalBearerAuthenticationAttribute>().Any())
        {
            operation.Security = [new OpenApiSecurityRequirement(), bearerRequirement];
            return;
        }

        var requiresAuthentication = metadata.OfType<IAuthorizeData>().Any() &&
                                     !metadata.OfType<IAllowAnonymous>().Any();
        if (requiresAuthentication) operation.Security = [bearerRequirement];
    }
}
