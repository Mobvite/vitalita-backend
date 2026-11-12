using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Results;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles the HTTP result of <c>POST /authentication/refresh</c>.
/// </summary>
public static class ActionResultFromRefreshTokenResultAssembler
{
    /// <summary>
    ///     Converts the refresh result into <c>200</c> (same body as the sign-in) or <c>401</c>.
    /// </summary>
    /// <param name="result">The application result.</param>
    /// <param name="controller">The controller used to build the result.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <returns>The HTTP action result.</returns>
    public static ActionResult ToActionResultFromRefreshTokenResult(
        Result<AuthenticatedUser, RefreshTokenError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<AuthenticatedUser, RefreshTokenError>.Success success =>
                controller.Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<AuthenticatedUser, RefreshTokenError>.Failure { Error: RefreshTokenError.InactiveAccount } =>
                controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                    title: localizer["AccountInactive"].Value),

            _ => controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                title: localizer["SessionUserNotFound"].Value)
        };
}
