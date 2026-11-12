using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Profiles.Interfaces.REST.Transform;
using Vitalita.API.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST;

/// <summary>
///     Caregiver profiles.
/// </summary>
/// <param name="commandService">The caregiver profile command service.</param>
/// <param name="queryService">The caregiver profile query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Caregiver Profiles")]
public class CaregiverProfilesController(
    ICaregiverProfileCommandService commandService,
    ICaregiverProfileQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "CaregiverProfileNotFound";

    /// <summary>
    ///     Gets the caregiver profile of a user, or lists every profile (administrators only).
    /// </summary>
    /// <remarks>
    ///     With <c>?userId=</c>: an array with zero or one profile; only the user themselves (or an administrator)
    ///     can ask for it. Without filters: every profile, administrators only.
    /// </remarks>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching profiles (a plain array).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CaregiverProfileResource>), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetCaregiverProfiles([FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var result = userId is { } value
            ? await queryService.Handle(new GetCaregiverProfilesByUserIdQuery(requester, value), cancellationToken)
            : await queryService.Handle(new GetAllCaregiverProfilesQuery(requester), cancellationToken);
        return result.Fold<ActionResult>(
            profiles => Ok(profiles.Select(CaregiverProfileResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a caregiver profile by identifier.
    /// </summary>
    /// <remarks>
    ///     The owner, administrators and family members with an active access to an older adult in the charge of
    ///     this caregiver can read it.
    /// </remarks>
    /// <param name="id">Profile identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CaregiverProfileResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetCaregiverProfileById(int id, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetCaregiverProfileByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            profile => Ok(CaregiverProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Creates the profile of a caregiver.
    /// </summary>
    /// <remarks>
    ///     A user with the <c>Caregiver</c> role creates only their own profile (<c>userId</c> = token
    ///     <c>sub</c>); administrators can create it for any existing user. One profile per user (<c>409</c>).
    /// </remarks>
    /// <param name="resource">The profile data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created profile, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CaregiverProfileResource), StatusCodes.Status201Created,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> CreateCaregiverProfile([FromBody] SaveCaregiverProfileResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = CreateCaregiverProfileCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            profile => CreatedAtAction(nameof(GetCaregiverProfileById), new { id = profile.Id },
                CaregiverProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Replaces the data of a caregiver profile.
    /// </summary>
    /// <remarks>
    ///     Only the owner (or an administrator) can update it, and <c>userId</c> cannot change (<c>409</c>).
    /// </remarks>
    /// <param name="id">Profile identifier.</param>
    /// <param name="resource">The whole profile.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CaregiverProfileResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> UpdateCaregiverProfile(int id, [FromBody] SaveCaregiverProfileResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = UpdateCaregiverProfileCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            profile => Ok(CaregiverProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }
}
