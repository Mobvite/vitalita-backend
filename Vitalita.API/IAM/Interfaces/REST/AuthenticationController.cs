using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.Queries;
using Vitalita.API.IAM.Interfaces.REST.Resources;
using Vitalita.API.IAM.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.IAM.Interfaces.REST;

/// <summary>
///     Authentication endpoints (TS01): public sign-up and sign-in, and the current user and token refresh
///     (Bearer token required).
/// </summary>
/// <remarks>
///     <c>[AllowAnonymous]</c> is set per action: on the controller it would override the <c>[Authorize]</c> of
///     <see cref="GetCurrentUser" /> and <see cref="RefreshToken" />.
/// </remarks>
/// <param name="userCommandService">The user command service.</param>
/// <param name="userQueryService">The user query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Authentication")]
public class AuthenticationController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    /// <summary>
    ///     Registers a Caregiver or FamilyMember account.
    /// </summary>
    /// <remarks>
    ///     The role defaults to <c>Caregiver</c>. <c>Admin</c> is rejected with <c>400</c>. The password is stored as
    ///     a BCrypt hash and never returned.
    /// </remarks>
    /// <param name="resource">Email, password and optional role.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created user, with its location in <c>/api/v1/users/{id}</c>.</returns>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> SignUp([FromBody] SignUpResource resource, CancellationToken cancellationToken)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await userCommandService.Handle(command, cancellationToken);
        return ActionResultFromSignUpResultAssembler.ToActionResultFromSignUpResult(result, this, localizer);
    }

    /// <summary>
    ///     Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <remarks>
    ///     Wrong credentials and inactive accounts return <c>401</c>. The token expires after
    ///     <c>Authentication:Jwt:ExpirationMinutes</c> (15 minutes at most).
    /// </remarks>
    /// <param name="resource">Email and password.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user id, email, role, status and token.</returns>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> SignIn([FromBody] SignInResource resource, CancellationToken cancellationToken)
    {
        var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await userCommandService.Handle(command, cancellationToken);
        return ActionResultFromSignInResultAssembler.ToActionResultFromSignInResult(result, this, localizer);
    }

    /// <summary>
    ///     Gets the user of the Bearer token.
    /// </summary>
    /// <remarks>
    ///     The user is read again from the database: <c>401</c> when the token is missing, invalid or expired, and
    ///     also when the user no longer exists or was deactivated.
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user (<c>id</c>, <c>email</c>, <c>role</c>, <c>status</c>, <c>createdAt</c>, <c>lastLoginAt</c>).</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId) return Challenge();

        var result = await userQueryService.Handle(new GetCurrentUserQuery(userId), cancellationToken);
        return ActionResultFromCurrentUserResultAssembler.ToActionResultFromCurrentUserResult(result, this, localizer);
    }

    /// <summary>
    ///     Issues a new access token for the user of a valid Bearer token.
    /// </summary>
    /// <remarks>
    ///     No body. The user is checked again in the database, so a deactivated user cannot extend the session
    ///     (<c>401</c>). The response has the same shape as the sign-in, and the new token lasts
    ///     <c>Authentication:Jwt:ExpirationMinutes</c> (15 minutes at most).
    /// </remarks>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user id, email, role, status and the new token.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId) return Challenge();

        var result = await userCommandService.Handle(new RefreshTokenCommand(userId), cancellationToken);
        return ActionResultFromRefreshTokenResultAssembler.ToActionResultFromRefreshTokenResult(result, this,
            localizer);
    }
}
