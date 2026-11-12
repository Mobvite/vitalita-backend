using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts the Monitoring application errors into RFC 7807 responses with localized titles.
/// </summary>
/// <remarks>
///     Business rule violations on a field (instant in the future, value out of range, schedule times) are
///     <c>400</c> validation problems on that field, like DataAnnotations errors.
/// </remarks>
public static class ActionResultFromMonitoringErrorAssembler
{
    /// <summary>
    ///     Converts a query error into <c>403</c> or <c>404</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="notFoundKey">The <c>.resx</c> key of the not found title of the resource.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(MonitoringQueryError error, string notFoundKey,
        ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            MonitoringQueryError.NotFound => Problem(controller, StatusCodes.Status404NotFound, localizer[notFoundKey]),
            _ => Forbidden(controller, localizer)
        };

    /// <summary>
    ///     Converts a daily report command error into <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(DailyReportCommandError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            DailyReportCommandError.NotFound =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["DailyReportNotFound"]),
            DailyReportCommandError.Forbidden => Forbidden(controller, localizer),
            DailyReportCommandError.OlderAdultNotFound => OlderAdultNotFound(controller, localizer),
            DailyReportCommandError.OlderAdultChangeNotAllowed => OlderAdultChange(controller, localizer),
            DailyReportCommandError.InstantInFuture =>
                FieldProblem(controller, "reportDate", localizer["InstantInFuture"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts a vital sign command error into <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="type">The type sent by the client, used to show its range.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(VitalSignCommandError error, VitalSignType type,
        ControllerBase controller, IStringLocalizer<SharedResource> localizer)
    {
        var rule = VitalSignRule.For(type);
        return error switch
        {
            VitalSignCommandError.NotFound =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["VitalSignNotFound"]),
            VitalSignCommandError.Forbidden => Forbidden(controller, localizer),
            VitalSignCommandError.OlderAdultNotFound => OlderAdultNotFound(controller, localizer),
            VitalSignCommandError.OlderAdultChangeNotAllowed => OlderAdultChange(controller, localizer),
            VitalSignCommandError.ValueOutOfRange => FieldProblem(controller, "value",
                localizer["VitalSignValueOutOfRange", Format(rule.Min), Format(rule.Max), rule.Unit]),
            VitalSignCommandError.SecondaryValueRequired =>
                FieldProblem(controller, "secondaryValue", localizer["DiastolicRequired"]),
            VitalSignCommandError.SecondaryValueOutOfRange => FieldProblem(controller, "secondaryValue",
                localizer["DiastolicOutOfRange", Format(rule.SecondaryMin ?? 0), Format(rule.SecondaryMax ?? 0),
                    rule.Unit]),
            VitalSignCommandError.SecondaryValueNotAllowed =>
                FieldProblem(controller, "secondaryValue", localizer["SecondaryValueNotAllowed"]),
            VitalSignCommandError.SystolicNotGreaterThanDiastolic =>
                FieldProblem(controller, "value", localizer["SystolicNotGreaterThanDiastolic"]),
            VitalSignCommandError.InstantInFuture =>
                FieldProblem(controller, "measuredAt", localizer["InstantInFuture"]),
            _ => ServerError(controller, localizer)
        };
    }

    /// <summary>
    ///     Converts a medication command error into <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(MedicationCommandError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            MedicationCommandError.NotFound =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["MedicationNotFound"]),
            MedicationCommandError.Forbidden => Forbidden(controller, localizer),
            MedicationCommandError.OlderAdultNotFound => OlderAdultNotFound(controller, localizer),
            MedicationCommandError.OlderAdultChangeNotAllowed => OlderAdultChange(controller, localizer),
            MedicationCommandError.InvalidScheduleTimes =>
                FieldProblem(controller, "scheduleTimes", localizer["ScheduleTimesLimit"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Creates the <c>403</c> response used when a requester cannot read or write the records of an older adult.
    /// </summary>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ObjectResult Forbidden(ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        Problem(controller, StatusCodes.Status403Forbidden, localizer["MonitoringAccessForbidden"]);

    private static ObjectResult OlderAdultNotFound(ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        Problem(controller, StatusCodes.Status400BadRequest, localizer["OlderAdultDoesNotExist"]);

    private static ObjectResult OlderAdultChange(ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        Problem(controller, StatusCodes.Status409Conflict, localizer["MonitoringOlderAdultChangeNotAllowed"]);

    private static string Format(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static ObjectResult Problem(ControllerBase controller, int statusCode, LocalizedString title) =>
        controller.Problem(statusCode: statusCode, title: title.Value);

    private static ActionResult FieldProblem(ControllerBase controller, string field, LocalizedString message)
    {
        controller.ModelState.AddModelError(field, message.Value);
        return controller.ValidationProblem(controller.ModelState);
    }

    private static ObjectResult ServerError(ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        controller.Problem(statusCode: StatusCodes.Status500InternalServerError,
            title: localizer["UnexpectedServerError"].Value,
            detail: localizer["UnexpectedErrorProcessingRequest"].Value);
}
