using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Queries;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Interfaces.REST.Resources;
using Vitalita.API.IAM.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.IAM.Interfaces.REST;

/// <summary>
///     User accounts.
/// </summary>
/// <param name="userQueryService">The user query service.</param>
/// <param name="authorizationService">Authorization service, used by the admin listing.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Users")]
public class UsersController(
    IUserQueryService userQueryService,
    IAuthorizationService authorizationService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    /// <summary>
    ///     Looks up users by email (public), or lists every user (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         With <c>?email=</c>: public lookup used by the frontend to know if an email is in use. It returns an
    ///         array with zero or one element that only has <c>id</c> and <c>email</c> (never the role, status or
    ///         password). This is a compromise kept for compatibility with the frontend.
    ///     </para>
    ///     <para>Without <c>email</c>: requires a Bearer token with the <c>Admin</c> role and returns full users.</para>
    /// </remarks>
    /// <param name="email">Email to look for. When present, the request is public.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>[{id, email}]</c> for the lookup, or the full users for administrators.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<UserResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetUsers([FromQuery] string? email, CancellationToken cancellationToken)
    {
        if (Request.Query.ContainsKey(nameof(email)))
        {
            if (!EmailAddress.IsValid(email))
            {
                ModelState.AddModelError(nameof(email), localizer["InvalidEmail"].Value);
                return ValidationProblem(ModelState);
            }

            var matches = await userQueryService.Handle(new GetUsersByEmailQuery(new EmailAddress(email!)),
                cancellationToken);
            return Ok(matches.Select(UserEmailResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var authorization = await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.AdminOnly);
        if (!authorization.Succeeded)
            return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        var users = await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken);
        return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    ///     Gets a user by identifier.
    /// </summary>
    /// <remarks>Administrators can read any user; other users can only read themselves (<c>403</c> otherwise).</remarks>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        if (!User.IsAdmin() && User.GetUserId() != id)
            return Problem(statusCode: StatusCodes.Status403Forbidden, title: localizer["UserAccessForbidden"].Value);

        var user = await userQueryService.Handle(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: localizer["UserNotFound"].Value);

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
}
