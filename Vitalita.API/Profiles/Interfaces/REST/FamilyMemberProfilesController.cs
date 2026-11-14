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
///     Family member profiles.
/// </summary>
/// <param name="commandService">The family member profile command service.</param>
/// <param name="queryService">The family member profile query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Family Member Profiles")]
public class FamilyMemberProfilesController(
    IFamilyMemberProfileCommandService commandService,
    IFamilyMemberProfileQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "FamilyMemberProfileNotFound";

    /// <summary>
    ///     Gets family member profiles by identifiers or by user, or lists every profile (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?id=1&amp;id=2</c>: the profiles the requester can read (their own, and for caregivers the ones
    ///         linked to their older adults); the others are left out. It takes precedence over <c>userId</c>.
    ///     </para>
    ///     <para><c>?userId=</c>: zero or one profile; only the user themselves (or an administrator).</para>
    ///     <para>Without filters: every profile, administrators only.</para>
    /// </remarks>
    /// <param name="ids">Profile identifiers (repeat the key).</param>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching profiles (a plain array).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FamilyMemberProfileResource>), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetFamilyMemberProfiles([FromQuery(Name = "id")] int[] ids,
        [FromQuery] int? userId, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        if (ids.Length > 0)
        {
            var visible = await queryService.Handle(new GetFamilyMemberProfilesByIdsQuery(requester, ids),
                cancellationToken);
            return Ok(visible.Select(FamilyMemberProfileResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var result = userId is { } value
            ? await queryService.Handle(new GetFamilyMemberProfilesByUserIdQuery(requester, value), cancellationToken)
            : await queryService.Handle(new GetAllFamilyMemberProfilesQuery(requester), cancellationToken);
        return result.Fold<ActionResult>(
            profiles => Ok(profiles.Select(FamilyMemberProfileResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a family member profile by identifier.
    /// </summary>
    /// <remarks>
    ///     The owner, administrators and caregivers with an older adult linked to this family member can read it.
    /// </remarks>
    /// <param name="id">Profile identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FamilyMemberProfileResource), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetFamilyMemberProfileById(int id, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetFamilyMemberProfileByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            profile => Ok(FamilyMemberProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Creates the profile of a family member.
    /// </summary>
    /// <remarks>
    ///     A user with the <c>FamilyMember</c> role creates only their own profile, and only when their email has
    ///     a pending, unexpired invitation (<c>403</c> otherwise). Administrators can create it for any existing
    ///     user. One profile per user (<c>409</c>).
    /// </remarks>
    /// <param name="resource">The profile data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created profile, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FamilyMemberProfileResource), StatusCodes.Status201Created,
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
    public async Task<ActionResult> CreateFamilyMemberProfile([FromBody] SaveFamilyMemberProfileResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = CreateFamilyMemberProfileCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            profile => CreatedAtAction(nameof(GetFamilyMemberProfileById), new { id = profile.Id },
                FamilyMemberProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Replaces the data of a family member profile.
    /// </summary>
    /// <remarks>Only the owner (or an administrator) can update it, and <c>userId</c> cannot change (<c>409</c>).</remarks>
    /// <param name="id">Profile identifier.</param>
    /// <param name="resource">The whole profile.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FamilyMemberProfileResource), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
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
    public async Task<ActionResult> UpdateFamilyMemberProfile(int id,
        [FromBody] SaveFamilyMemberProfileResource resource, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command =
            UpdateFamilyMemberProfileCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            profile => Ok(FamilyMemberProfileResourceFromEntityAssembler.ToResourceFromEntity(profile)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }
}
