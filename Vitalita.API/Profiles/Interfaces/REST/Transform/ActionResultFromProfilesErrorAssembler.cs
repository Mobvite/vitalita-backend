using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts the Profiles application errors into RFC 7807 responses with localized titles.
/// </summary>
/// <remarks>
///     Business rule violations on a field (birth date in the future, missing family member to accept) are
///     <c>400</c> validation problems on that field, like DataAnnotations errors.
/// </remarks>
public static class ActionResultFromProfilesErrorAssembler
{
    /// <summary>
    ///     Converts a query error into <c>403</c> or <c>404</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="notFoundKey">The <c>.resx</c> key of the not found title of the resource.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(ProfileQueryError error, string notFoundKey,
        ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            ProfileQueryError.NotFound => Problem(controller, StatusCodes.Status404NotFound, localizer[notFoundKey]),
            _ => Forbidden(controller, localizer)
        };

    /// <summary>
    ///     Converts a caregiver profile creation error into <c>400</c>, <c>403</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(CreateCaregiverProfileError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            CreateCaregiverProfileError.Forbidden =>
                Problem(controller, StatusCodes.Status403Forbidden, localizer["ProfileCreationForbidden"]),
            CreateCaregiverProfileError.UserNotFound =>
                Problem(controller, StatusCodes.Status400BadRequest, localizer["ProfileUserNotFound"]),
            CreateCaregiverProfileError.DuplicateProfile =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["ProfileAlreadyExists"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts a family member profile creation error into <c>400</c>, <c>403</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(CreateFamilyMemberProfileError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            CreateFamilyMemberProfileError.Forbidden =>
                Problem(controller, StatusCodes.Status403Forbidden, localizer["ProfileCreationForbidden"]),
            CreateFamilyMemberProfileError.NoValidInvitation =>
                Problem(controller, StatusCodes.Status403Forbidden, localizer["NoValidInvitation"]),
            CreateFamilyMemberProfileError.UserNotFound =>
                Problem(controller, StatusCodes.Status400BadRequest, localizer["ProfileUserNotFound"]),
            CreateFamilyMemberProfileError.DuplicateProfile =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["ProfileAlreadyExists"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts a profile update error into <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="notFoundKey">The <c>.resx</c> key of the not found title of the profile.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(UpdateProfileError error, string notFoundKey,
        ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            UpdateProfileError.NotFound => Problem(controller, StatusCodes.Status404NotFound, localizer[notFoundKey]),
            UpdateProfileError.Forbidden => Forbidden(controller, localizer),
            UpdateProfileError.OwnerChangeNotAllowed =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["ProfileOwnerChangeNotAllowed"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts an older adult creation error into <c>400</c>, <c>403</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(CreateOlderAdultError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            CreateOlderAdultError.Forbidden => Forbidden(controller, localizer),
            CreateOlderAdultError.CaregiverNotFound =>
                Problem(controller, StatusCodes.Status400BadRequest, localizer["CaregiverProfileDoesNotExist"]),
            CreateOlderAdultError.BirthDateInFuture =>
                FieldProblem(controller, "birthDate", localizer["BirthDateInFuture"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts an older adult update error into <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(UpdateOlderAdultError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            UpdateOlderAdultError.NotFound =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["OlderAdultNotFound"]),
            UpdateOlderAdultError.Forbidden => Forbidden(controller, localizer),
            UpdateOlderAdultError.OwnerChangeNotAllowed =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["OlderAdultCaregiverChangeNotAllowed"]),
            UpdateOlderAdultError.BirthDateInFuture =>
                FieldProblem(controller, "birthDate", localizer["BirthDateInFuture"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts an invitation creation error into <c>400</c>, <c>403</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(CreateFamilyAccessError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            CreateFamilyAccessError.Forbidden => Forbidden(controller, localizer),
            CreateFamilyAccessError.OlderAdultNotFound =>
                Problem(controller, StatusCodes.Status400BadRequest, localizer["OlderAdultDoesNotExist"]),
            CreateFamilyAccessError.DuplicateInvitation =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["InvitationAlreadyExists"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts a family access update error into <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ActionResult ToActionResult(UpdateFamilyAccessError error, ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        error switch
        {
            UpdateFamilyAccessError.NotFound =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["FamilyAccessNotFound"]),
            UpdateFamilyAccessError.Forbidden => Forbidden(controller, localizer),
            UpdateFamilyAccessError.FamilyMemberRequired =>
                FieldProblem(controller, "familyMemberId", localizer["FamilyMemberRequired"]),
            UpdateFamilyAccessError.FamilyMemberNotFound =>
                Problem(controller, StatusCodes.Status400BadRequest, localizer["FamilyMemberProfileDoesNotExist"]),
            UpdateFamilyAccessError.InvitationNotAcceptable =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["InvitationNotAcceptable"]),
            UpdateFamilyAccessError.InvalidStatusTransition =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["InvalidAccessStatusTransition"]),
            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Creates the <c>403</c> response used when a requester cannot read or change a Profiles resource.
    /// </summary>
    /// <param name="controller">The controller that handles the request.</param>
    /// <param name="localizer">The localizer.</param>
    /// <returns>The problem response.</returns>
    public static ObjectResult Forbidden(ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        Problem(controller, StatusCodes.Status403Forbidden, localizer["ProfileAccessForbidden"]);

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
