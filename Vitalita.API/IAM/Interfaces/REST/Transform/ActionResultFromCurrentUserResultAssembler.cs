using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles the HTTP result of <c>GET /authentication/me</c>.
/// </summary>
public static class ActionResultFromCurrentUserResultAssembler
{
    /// <summary>
    ///     Converts the current user result into <c>200</c> or <c>401</c>.
    /// </summary>
    /// <param name="result">The application result.</param>
    /// <param name="controller">The controller used to build the result.</param>
    /// <param name="localizer">The string localizer.</param>
    /// <returns>The HTTP action result.</returns>
    public static ActionResult ToActionResultFromCurrentUserResult(
        Result<User, CurrentUserError> result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer) =>
        result switch
        {
            Result<User, CurrentUserError>.Success success =>
                controller.Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),

            Result<User, CurrentUserError>.Failure { Error: CurrentUserError.InactiveAccount } =>
                controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                    title: localizer["AccountInactive"].Value),

            _ => controller.Problem(statusCode: StatusCodes.Status401Unauthorized,
                title: localizer["SessionUserNotFound"].Value)
        };
}
