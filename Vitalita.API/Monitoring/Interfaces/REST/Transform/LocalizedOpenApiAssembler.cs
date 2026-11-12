using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Assembles localized OpenAPI metadata (summaries, descriptions, response texts and examples)
///     for the Monitoring REST endpoints and resources.
/// </summary>
public static class LocalizedOpenApiAssembler
{
    private static readonly IReadOnlySet<string> Controllers =
        new HashSet<string> { "DailyReports", "VitalSigns", "Medications" };

    private static readonly IReadOnlyDictionary<string, string> ListResponses = new Dictionary<string, string>
    {
        ["200"] = "MonitoringListResponse",
        ["400"] = "MonitoringValidationErrorResponse",
        ["401"] = "MonitoringUnauthorizedResponse",
        ["403"] = "MonitoringForbiddenResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> ItemResponses = new Dictionary<string, string>
    {
        ["200"] = "MonitoringItemResponse",
        ["401"] = "MonitoringUnauthorizedResponse",
        ["403"] = "MonitoringForbiddenResponse",
        ["404"] = "MonitoringNotFoundResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> CreateResponses = new Dictionary<string, string>
    {
        ["201"] = "MonitoringCreatedResponse",
        ["400"] = "MonitoringValidationErrorResponse",
        ["401"] = "MonitoringUnauthorizedResponse",
        ["403"] = "MonitoringForbiddenResponse",
        ["409"] = "MonitoringConflictResponse",
        ["500"] = "MonitoringServerErrorResponse"
    };

    private static readonly IReadOnlyDictionary<string, string> UpdateResponses = new Dictionary<string, string>
    {
        ["200"] = "MonitoringUpdatedResponse",
        ["400"] = "MonitoringValidationErrorResponse",
        ["401"] = "MonitoringUnauthorizedResponse",
        ["403"] = "MonitoringForbiddenResponse",
        ["404"] = "MonitoringNotFoundResponse",
        ["409"] = "MonitoringConflictResponse",
        ["500"] = "MonitoringServerErrorResponse"
    };

    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> OperationResponses = new()
    {
        ["GetDailyReports"] = ListResponses,
        ["GetDailyReportById"] = ItemResponses,
        ["CreateDailyReport"] = CreateResponses,
        ["UpdateDailyReport"] = UpdateResponses,
        ["GetVitalSigns"] = ListResponses,
        ["GetVitalSignById"] = ItemResponses,
        ["CreateVitalSign"] = CreateResponses,
        ["UpdateVitalSign"] = UpdateResponses,
        ["GetMedications"] = ListResponses,
        ["GetMedicationById"] = ItemResponses,
        ["CreateMedication"] = CreateResponses,
        ["UpdateMedication"] = UpdateResponses
    };

    private static readonly Dictionary<Type, string> SchemaResources = new()
    {
        [typeof(DailyReportResource)] = "MonitoringDailyReportResourceSchema",
        [typeof(SaveDailyReportResource)] = "MonitoringSaveDailyReportResourceSchema",
        [typeof(VitalSignResource)] = "MonitoringVitalSignResourceSchema",
        [typeof(SaveVitalSignResource)] = "MonitoringSaveVitalSignResourceSchema",
        [typeof(MedicationResource)] = "MonitoringMedicationResourceSchema",
        [typeof(SaveMedicationResource)] = "MonitoringSaveMedicationResourceSchema"
    };

    private static readonly Dictionary<string, (string Description, JsonNode? Example)> PropertyResources = new()
    {
        ["id"] = ("MonitoringIdProperty", JsonValue.Create(1)),
        ["olderAdultId"] = ("MonitoringOlderAdultIdProperty", JsonValue.Create(1)),
        ["recordedByUserId"] = ("MonitoringRecordedByUserIdProperty", JsonValue.Create(1)),
        ["reportDate"] = ("MonitoringReportDateProperty", JsonValue.Create("2026-11-11T20:00:00Z")),
        ["mood"] = ("MonitoringMoodProperty", JsonValue.Create("Good")),
        ["generalCondition"] = ("MonitoringGeneralConditionProperty", JsonValue.Create("Stable")),
        ["observations"] = ("MonitoringObservationsProperty", JsonValue.Create("Durmió bien y almorzó completo.")),
        ["type"] = ("MonitoringTypeProperty", JsonValue.Create("BloodPressure")),
        ["value"] = ("MonitoringValueProperty", JsonValue.Create(120)),
        ["secondaryValue"] = ("MonitoringSecondaryValueProperty", JsonValue.Create(80)),
        ["unit"] = ("MonitoringUnitProperty", JsonValue.Create("mmHg")),
        ["measuredAt"] = ("MonitoringMeasuredAtProperty", JsonValue.Create("2026-11-11T13:00:00Z")),
        ["name"] = ("MonitoringNameProperty", JsonValue.Create("Losartán")),
        ["dosage"] = ("MonitoringDosageProperty", JsonValue.Create("50 mg")),
        ["frequency"] = ("MonitoringFrequencyProperty", JsonValue.Create("Cada 12 horas")),
        ["scheduleTimes"] = ("MonitoringScheduleTimesProperty", new JsonArray("08:00", "20:00")),
        ["instructions"] = ("MonitoringInstructionsProperty", JsonValue.Create("Tomar con el almuerzo.")),
        ["active"] = ("MonitoringActiveProperty", JsonValue.Create(true))
    };


    /// <summary>
    ///     Localizes the summary, description, operation id and response descriptions of the Monitoring operations.
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
        operation.Summary = localizer[$"Monitoring{action.ActionName}Summary"].Value;
        operation.Description = localizer[$"Monitoring{action.ActionName}Description"].Value;
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
    ///     Localizes the description of the Monitoring resource schemas and adds examples to their properties.
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
