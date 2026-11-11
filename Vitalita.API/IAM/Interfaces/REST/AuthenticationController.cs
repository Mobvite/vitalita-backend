using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Interfaces.REST.Resources;
using Vitalita.API.IAM.Interfaces.REST.Transform;
using Vitalita.API.Resources;

namespace Vitalita.API.IAM.Interfaces.REST;

/// <summary>
///     Public sign-up and sign-in endpoints (TS01).
/// </summary>
/// <param name="userCommandService">The user command service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Authentication")]
public class AuthenticationController(
    IUserCommandService userCommandService,
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status201Created)]
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
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
}
