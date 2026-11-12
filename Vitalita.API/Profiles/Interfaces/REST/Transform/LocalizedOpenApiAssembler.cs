using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Assembles localized OpenAPI metadata (summaries, descriptions, response texts and examples)
///     for the Profiles REST endpoints and resources.
/// </summary>
public static class LocalizedOpenApiAssembler
{
    private static readonly IReadOnlySet<string> Controllers =
        new HashSet<string> { "CaregiverProfiles", "FamilyMemberProfiles", "OlderAdults", "FamilyAccess" };

    private static readonly IReadOnlyDictionary<string, string> ListResponses = new Dictionary<string, string>
    {
        ["200"] = "ProfilesListResponse",
        ["400"] = "ProfilesValidationErrorResponse",
        ["401"] = "ProfilesUnauthorizedResponse",
        ["403"] = "ProfilesForbiddenResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> ItemResponses = new Dictionary<string, string>
    {
        ["200"] = "ProfilesItemResponse",
        ["401"] = "ProfilesUnauthorizedResponse",
        ["403"] = "ProfilesForbiddenResponse",
        ["404"] = "ProfilesNotFoundResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> CreateResponses = new Dictionary<string, string>
    {
        ["201"] = "ProfilesCreatedResponse",
        ["400"] = "ProfilesValidationErrorResponse",
        ["401"] = "ProfilesUnauthorizedResponse",
        ["403"] = "ProfilesForbiddenResponse",
        ["409"] = "ProfilesConflictResponse",
        ["500"] = "ProfilesServerErrorResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> UpdateResponses = new Dictionary<string, string>
    {
        ["200"] = "ProfilesUpdatedResponse",
        ["400"] = "ProfilesValidationErrorResponse",
        ["401"] = "ProfilesUnauthorizedResponse",
        ["403"] = "ProfilesForbiddenResponse",
        ["404"] = "ProfilesNotFoundResponse",
        ["409"] = "ProfilesConflictResponse",
        ["500"] = "ProfilesServerErrorResponse"
    };

    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> OperationResponses = new()
    {
        ["GetCaregiverProfiles"] = ListResponses,
        ["GetCaregiverProfileById"] = ItemResponses,
        ["CreateCaregiverProfile"] = CreateResponses,
        ["UpdateCaregiverProfile"] = UpdateResponses,
        ["GetFamilyMemberProfiles"] = ListResponses,
        ["GetFamilyMemberProfileById"] = ItemResponses,
        ["CreateFamilyMemberProfile"] = CreateResponses,
        ["UpdateFamilyMemberProfile"] = UpdateResponses,
        ["GetOlderAdults"] = ListResponses,
        ["GetOlderAdultById"] = ItemResponses,
        ["CreateOlderAdult"] = CreateResponses,
        ["UpdateOlderAdult"] = UpdateResponses,
        ["GetFamilyAccesses"] = ListResponses,
        ["GetFamilyAccessById"] = ItemResponses,
        ["CreateFamilyAccess"] = CreateResponses,
        ["UpdateFamilyAccess"] = UpdateResponses
    };

    private static readonly Dictionary<Type, string> SchemaResources = new()
    {
        [typeof(CaregiverProfileResource)] = "ProfilesCaregiverProfileResourceSchema",
        [typeof(SaveCaregiverProfileResource)] = "ProfilesSaveCaregiverProfileResourceSchema",
        [typeof(FamilyMemberProfileResource)] = "ProfilesFamilyMemberProfileResourceSchema",
        [typeof(SaveFamilyMemberProfileResource)] = "ProfilesSaveFamilyMemberProfileResourceSchema",
        [typeof(OlderAdultResource)] = "ProfilesOlderAdultResourceSchema",
        [typeof(SaveOlderAdultResource)] = "ProfilesSaveOlderAdultResourceSchema",
        [typeof(EmergencyContactResource)] = "ProfilesEmergencyContactResourceSchema",
        [typeof(FamilyAccessResource)] = "ProfilesFamilyAccessResourceSchema",
        [typeof(InvitationResource)] = "ProfilesInvitationResourceSchema",
        [typeof(CreateFamilyAccessResource)] = "ProfilesCreateFamilyAccessResourceSchema",
        [typeof(UpdateFamilyAccessResource)] = "ProfilesUpdateFamilyAccessResourceSchema"
    };

    private static readonly Dictionary<string, (string Description, JsonNode? Example)> PropertyResources = new()
    {
        ["id"] = ("ProfilesIdProperty", JsonValue.Create(1)),
        ["userId"] = ("ProfilesUserIdProperty", JsonValue.Create(1)),
        ["firstName"] = ("ProfilesFirstNameProperty", JsonValue.Create("Andrea")),
        ["lastName"] = ("ProfilesLastNameProperty", JsonValue.Create("Mendoza")),
        ["phoneNumber"] = ("ProfilesPhoneNumberProperty", JsonValue.Create("987654321")),
        ["professionalTitle"] = ("ProfilesProfessionalTitleProperty", JsonValue.Create("Licenciada en Enfermería")),
        ["bio"] = ("ProfilesBioProperty",
            JsonValue.Create("Enfermera con 12 años de experiencia en cuidado geriátrico domiciliario.")),
        ["photoUrl"] = ("ProfilesPhotoUrlProperty", JsonValue.Create("")),
        ["certifications"] = ("ProfilesCertificationsProperty",
            new JsonArray("Cuidado geriátrico", "Primeros auxilios")),
        ["caregiverId"] = ("ProfilesCaregiverIdProperty", JsonValue.Create(1)),
        ["birthDate"] = ("ProfilesBirthDateProperty", JsonValue.Create("1948-03-12")),
        ["documentNumber"] = ("ProfilesDocumentNumberProperty", JsonValue.Create("40218463")),
        ["bloodType"] = ("ProfilesBloodTypeProperty", JsonValue.Create("O+")),
        ["allergies"] = ("ProfilesAllergiesProperty", new JsonArray("Penicilina")),
        ["chronicConditions"] = ("ProfilesChronicConditionsProperty",
            new JsonArray("Hipertensión arterial", "Diabetes tipo 2")),
        ["emergencyContact"] = ("ProfilesEmergencyContactProperty", null),
        ["name"] = ("ProfilesContactNameProperty", JsonValue.Create("Renzo Rodríguez")),
        ["relationship"] = ("ProfilesRelationshipProperty", JsonValue.Create("Son")),
        ["observations"] = ("ProfilesObservationsProperty",
            JsonValue.Create("Requiere asistencia para caminar distancias largas.")),
        ["status"] = ("ProfilesStatusProperty", JsonValue.Create("Active")),
        ["olderAdultId"] = ("ProfilesOlderAdultIdProperty", JsonValue.Create(1)),
        ["familyMemberId"] = ("ProfilesFamilyMemberIdProperty", JsonValue.Create(1)),
        ["invitedEmail"] = ("ProfilesInvitedEmailProperty", JsonValue.Create("carla.rodriguez@correo.pe")),
        ["invitationCode"] = ("ProfilesInvitationCodeProperty", JsonValue.Create("VITA-7KQ2M9XH")),
        ["invitedAt"] = ("ProfilesInvitedAtProperty", JsonValue.Create("2026-11-11T15:00:00Z")),
        ["acceptedAt"] = ("ProfilesAcceptedAtProperty", JsonValue.Create("2026-11-11T18:30:00Z")),
        ["expiresAt"] = ("ProfilesExpiresAtProperty", JsonValue.Create("2026-11-18T15:00:00Z"))
    };

    /// <summary>
    ///     Localizes the summary, description, operation id and response descriptions of the Profiles operations.
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
            !OperationResponses.TryGetValue(action.ActionName, out var responses))
            return Task.CompletedTask;

        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        operation.OperationId = action.ActionName;
        operation.Summary = localizer[$"Profiles{action.ActionName}Summary"].Value;
        operation.Description = localizer[$"Profiles{action.ActionName}Description"].Value;
        if (operation.Responses is null) return Task.CompletedTask;

        foreach (var (statusCode, resourceKey) in responses)
        {
            if (operation.Responses.TryGetValue(statusCode, out var response) &&
                response is OpenApiResponse openApiResponse)
                openApiResponse.Description = localizer[resourceKey].Value;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Localizes the description of the Profiles resource schemas and adds examples to their properties.
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
        if (property.Example is not null) schema.Example = property.Example.DeepClone();
        return Task.CompletedTask;
    }
}
