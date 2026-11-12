using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles the HTTP results of the user management commands (create, update, deactivate).
/// </summary>
public static class ActionResultFromUserManagementResultAssembler
{
    /// <summary>
    ///     Converts the create result into <c>201</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    public static ActionResult ToActionResultFromCreateUserResult(
        Result<User, CreateUserError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<User, CreateUserError>.Success success =>
                controller.CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = success.Value.Id },
                    UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<User, CreateUserError>.Failure { Error: CreateUserError.DuplicateEmail } =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["EmailAlreadyInUse"].Value),

            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts the update result into <c>200</c>, <c>403</c>, <c>404</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    public static ActionResult ToActionResultFromUpdateUserResult(
        Result<User, UpdateUserError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<User, UpdateUserError>.Success success =>
                controller.Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<User, UpdateUserError>.Failure failure => failure.Error switch
            {
                UpdateUserError.Forbidden =>
                    Problem(controller, StatusCodes.Status403Forbidden, localizer["UserAccessForbidden"].Value),
                UpdateUserError.RoleChangeNotAllowed =>
                    Problem(controller, StatusCodes.Status403Forbidden, localizer["RoleChangeNotAllowed"].Value),
                UpdateUserError.StatusChangeNotAllowed =>
                    Problem(controller, StatusCodes.Status403Forbidden, localizer["StatusChangeNotAllowed"].Value),
                UpdateUserError.NotFound =>
                    Problem(controller, StatusCodes.Status404NotFound, localizer["UserNotFound"].Value),
                UpdateUserError.DuplicateEmail =>
                    Problem(controller, StatusCodes.Status409Conflict, localizer["EmailAlreadyInUse"].Value),
                _ => ServerError(controller, localizer)
            },

            _ => ServerError(controller, localizer)
        };

    /// <summary>
    ///     Converts the deactivate result into <c>204</c>, <c>404</c> or <c>409</c>.
    /// </summary>
    public static ActionResult ToActionResultFromDeactivateUserResult(
        Result<User, DeactivateUserError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<User, DeactivateUserError>.Success => controller.NoContent(),

            Result<User, DeactivateUserError>.Failure { Error: DeactivateUserError.NotFound } =>
                Problem(controller, StatusCodes.Status404NotFound, localizer["UserNotFound"].Value),

            Result<User, DeactivateUserError>.Failure { Error: DeactivateUserError.CannotDeactivateSelf } =>
                Problem(controller, StatusCodes.Status409Conflict, localizer["CannotDeactivateSelf"].Value),

            _ => ServerError(controller, localizer)
        };

    private static ObjectResult Problem(ControllerBase controller, int statusCode, string title) =>
        controller.Problem(statusCode: statusCode, title: title);

    private static ObjectResult ServerError(ControllerBase controller, IStringLocalizer<SharedResource> localizer) =>
        controller.Problem(statusCode: StatusCodes.Status500InternalServerError,
            title: localizer["UnexpectedServerError"].Value,
            detail: localizer["UnexpectedErrorProcessingRequest"].Value);
}
