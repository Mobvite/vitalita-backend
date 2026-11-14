using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Interfaces.REST.Resources;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles the HTTP result of the public sign-up.
/// </summary>
public static class ActionResultFromSignUpResultAssembler
{
    /// <summary>
    ///     Converts the sign-up result into <c>201</c>, <c>400</c>, <c>409</c> or <c>500</c>.
    /// </summary>
    /// <param name="result">The application result.</param>
    /// <param name="controller">The controller used to build the result.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <returns>The HTTP action result.</returns>
    public static ActionResult ToActionResultFromSignUpResult(
        Result<User, SignUpError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<User, SignUpError>.Success success =>
                controller.CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = success.Value.Id },
                    UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<User, SignUpError>.Failure { Error: SignUpError.AdminRoleNotAllowed } =>
                ValidationProblem(controller, nameof(SignUpResource.Role), localizer["AdminSignUpNotAllowed"].Value),

            Result<User, SignUpError>.Failure { Error: SignUpError.DuplicateEmail } =>
                controller.Problem(statusCode: StatusCodes.Status409Conflict,
                    title: localizer["EmailAlreadyInUse"].Value),

            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: localizer["UnexpectedServerError"].Value,
                detail: localizer["UnexpectedErrorProcessingRequest"].Value)
        };

    private static ActionResult ValidationProblem(ControllerBase controller, string key, string message)
    {
        controller.ModelState.AddModelError(key, message);
        return controller.ValidationProblem(controller.ModelState);
    }
}
