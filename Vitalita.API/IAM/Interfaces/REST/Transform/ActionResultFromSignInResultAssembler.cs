using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Results;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles the HTTP result of the sign-in.
/// </summary>
public static class ActionResultFromSignInResultAssembler
{
    /// <summary>
    ///     Converts the sign-in result into <c>200</c> or <c>401</c>.
    /// </summary>
    /// <param name="result">The application result.</param>
    /// <param name="controller">The controller used to build the result.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <returns>The HTTP action result.</returns>
    public static ActionResult ToActionResultFromSignInResult(
        Result<AuthenticatedUser, SignInError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<AuthenticatedUser, SignInError>.Success success =>
                controller.Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<AuthenticatedUser, SignInError>.Failure { Error: SignInError.InactiveAccount } =>
                controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                    title: localizer["AccountInactive"].Value),

            _ => controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                title: localizer["InvalidCredentials"].Value)
        };
}
