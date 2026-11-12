using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.IAM.Interfaces.REST.Resources;
using Vitalita.API.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles localized OpenAPI metadata (summaries, descriptions, response texts and examples)
///     for the IAM REST endpoints and resources.
/// </summary>
public static class LocalizedOpenApiAssembler
{
    private static readonly IReadOnlySet<string> Controllers =
        new HashSet<string> { "Authentication", "Users" };

    private static readonly Dictionary<string, (string Summary, string Description,
        IReadOnlyDictionary<string, string> Responses)> OperationResources =
        new()
        {
            ["SignUp"] = ("IamSignUpSummary", "IamSignUpDescription", new Dictionary<string, string>
            {
                ["201"] = "IamUserCreatedResponse",
                ["400"] = "IamValidationErrorResponse",
                ["409"] = "IamEmailInUseResponse",
                ["500"] = "IamServerErrorResponse"
            }),
            ["SignIn"] = ("IamSignInSummary", "IamSignInDescription", new Dictionary<string, string>
            {
                ["200"] = "IamSignedInResponse",
                ["400"] = "IamValidationErrorResponse",
                ["401"] = "IamInvalidCredentialsResponse"
            }),
            ["GetCurrentUser"] = ("IamGetCurrentUserSummary", "IamGetCurrentUserDescription",
                new Dictionary<string, string>
                {
                    ["200"] = "IamCurrentUserResponse",
                    ["401"] = "IamSessionInvalidResponse"
                }),
            ["RefreshToken"] = ("IamRefreshTokenSummary", "IamRefreshTokenDescription",
                new Dictionary<string, string>
                {
                    ["200"] = "IamTokenRefreshedResponse",
                    ["401"] = "IamSessionInvalidResponse"
                }),
            ["GetUsers"] = ("IamGetUsersSummary", "IamGetUsersDescription", new Dictionary<string, string>
            {
                ["200"] = "IamUsersResponse",
                ["400"] = "IamValidationErrorResponse",
                ["401"] = "IamUnauthorizedResponse",
                ["403"] = "IamAdminOnlyResponse"
            }),
            ["GetUserById"] = ("IamGetUserByIdSummary", "IamGetUserByIdDescription", new Dictionary<string, string>
            {
                ["200"] = "IamUserResponse",
                ["401"] = "IamUnauthorizedResponse",
                ["403"] = "IamOwnUserOrAdminResponse",
                ["404"] = "IamUserNotFoundResponse"
            }),
            ["CreateUser"] = ("IamCreateUserSummary", "IamCreateUserDescription", new Dictionary<string, string>
            {
                ["201"] = "IamUserCreatedResponse",
                ["400"] = "IamValidationErrorResponse",
                ["401"] = "IamUnauthorizedResponse",
                ["403"] = "IamAdminOnlyResponse",
                ["409"] = "IamEmailInUseResponse",
                ["500"] = "IamServerErrorResponse"
            }),
            ["UpdateUser"] = ("IamUpdateUserSummary", "IamUpdateUserDescription", new Dictionary<string, string>
            {
                ["200"] = "IamUserUpdatedResponse",
                ["400"] = "IamValidationErrorResponse",
                ["401"] = "IamUnauthorizedResponse",
                ["403"] = "IamUpdateForbiddenResponse",
                ["404"] = "IamUserNotFoundResponse",
                ["409"] = "IamEmailInUseResponse",
                ["500"] = "IamServerErrorResponse"
            }),
            ["DeactivateUser"] = ("IamDeactivateUserSummary", "IamDeactivateUserDescription",
                new Dictionary<string, string>
                {
                    ["204"] = "IamUserDeactivatedResponse",
                    ["401"] = "IamUnauthorizedResponse",
                    ["403"] = "IamAdminOnlyResponse",
                    ["404"] = "IamUserNotFoundResponse",
                    ["409"] = "IamCannotDeactivateSelfResponse"
                })
        };

    private static readonly Dictionary<Type, string> SchemaResources = new()
    {
        [typeof(SignUpResource)] = "IamSignUpResourceSchema",
        [typeof(SignInResource)] = "IamSignInResourceSchema",
        [typeof(AuthenticatedUserResource)] = "IamAuthenticatedUserResourceSchema",
        [typeof(UserResource)] = "IamUserResourceSchema",
        [typeof(UserEmailResource)] = "IamUserEmailResourceSchema",
        [typeof(CreateUserResource)] = "IamCreateUserResourceSchema",
        [typeof(UpdateUserResource)] = "IamUpdateUserResourceSchema"
    };

    private static readonly Dictionary<string, (string Description, JsonNode? Example)> PropertyResources = new()
    {
        ["id"] = ("IamUserIdProperty", JsonValue.Create(1)),
        ["email"] = ("IamEmailProperty", JsonValue.Create("andrea.mendoza@vitalita.pe")),
        ["password"] = ("IamPasswordProperty", JsonValue.Create("Vitalita123")),
        ["role"] = ("IamRoleProperty", JsonValue.Create("Caregiver")),
        ["status"] = ("IamStatusProperty", JsonValue.Create("Active")),
        ["token"] = ("IamTokenProperty", JsonValue.Create("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")),
        ["createdAt"] = ("IamCreatedAtProperty", JsonValue.Create("2026-11-11T15:00:00Z")),
        ["lastLoginAt"] = ("IamLastLoginAtProperty", JsonValue.Create("2026-11-11T15:05:00Z"))
    };

    /// <summary>
    ///     Localizes the summary, description, operation id and response descriptions of the IAM operations.
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
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action ||
            !Controllers.Contains(action.ControllerName) ||
            !OperationResources.TryGetValue(action.ActionName, out var resources))
            return Task.CompletedTask;

        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        operation.OperationId = action.ActionName;
        operation.Summary = localizer[resources.Summary].Value;
        operation.Description = localizer[resources.Description].Value;
        if (operation.Responses is null) return Task.CompletedTask;

        foreach (var (statusCode, resourceKey) in resources.Responses)
        {
            if (operation.Responses.TryGetValue(statusCode, out var response) &&
                response is OpenApiResponse openApiResponse)
                openApiResponse.Description = localizer[resourceKey].Value;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Localizes the description of the IAM resource schemas and adds examples to their properties.
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
        if (SchemaResources.TryGetValue(context.JsonTypeInfo.Type, out var schemaKey))
            schema.Description = localizer[schemaKey].Value;

        var declaringType = context.JsonPropertyInfo?.DeclaringType;
        if (declaringType is null || !SchemaResources.ContainsKey(declaringType) ||
            !PropertyResources.TryGetValue(context.JsonPropertyInfo!.Name, out var property))
            return Task.CompletedTask;

        schema.Description = localizer[property.Description].Value;
        schema.Example = property.Example?.DeepClone();
        return Task.CompletedTask;
    }
}
