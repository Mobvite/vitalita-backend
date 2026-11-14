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
///     Older adults followed by caregivers and family members.
/// </summary>
/// <param name="commandService">The older adult command service.</param>
/// <param name="queryService">The older adult query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Older Adults")]
public class OlderAdultsController(
    IOlderAdultCommandService commandService,
    IOlderAdultQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "OlderAdultNotFound";

    /// <summary>
    ///     Gets older adults by identifiers or by caregiver, or lists every older adult (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?id=1&amp;id=2</c>: the older adults the requester can read (in their charge, or with an active
    ///         family access); the others are left out. It takes precedence over <c>caregiverId</c>.
    ///     </para>
    ///     <para><c>?caregiverId=</c>: the older adults of the requester's own caregiver profile (<c>403</c> otherwise).</para>
    ///     <para>Without filters: every older adult, administrators only.</para>
    /// </remarks>
    /// <param name="ids">Older adult identifiers (repeat the key).</param>
    /// <param name="caregiverId">Caregiver profile identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching older adults (a plain array).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OlderAdultResource>), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetOlderAdults([FromQuery(Name = "id")] int[] ids, [FromQuery] int? caregiverId,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        if (ids.Length > 0)
        {
            var visible = await queryService.Handle(new GetOlderAdultsByIdsQuery(requester, ids), cancellationToken);
            return Ok(visible.Select(OlderAdultResourceFromEntityAssembler.ToResourceFromEntity));
        }

        var result = caregiverId is { } value
            ? await queryService.Handle(new GetOlderAdultsByCaregiverIdQuery(requester, value), cancellationToken)
            : await queryService.Handle(new GetAllOlderAdultsQuery(requester), cancellationToken);
        return result.Fold<ActionResult>(
            olderAdults => Ok(olderAdults.Select(OlderAdultResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets an older adult by identifier.
    /// </summary>
    /// <remarks>
    ///     The caregiver in charge, administrators and family members with an active access can read it.
    /// </remarks>
    /// <param name="id">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adult.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OlderAdultResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetOlderAdultById(int id, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetOlderAdultByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            olderAdult => Ok(OlderAdultResourceFromEntityAssembler.ToResourceFromEntity(olderAdult)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Registers an older adult.
    /// </summary>
    /// <remarks>
    ///     <c>caregiverId</c> must be the requester's own caregiver profile (administrators: any existing one).
    ///     <c>birthDate</c> (<c>yyyy-MM-dd</c>) cannot be later than today in Peru. A missing <c>status</c> means
    ///     <c>Active</c>. The plan limit is not checked by the API yet.
    /// </remarks>
    /// <param name="resource">The older adult data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created older adult, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OlderAdultResource), StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> CreateOlderAdult([FromBody] SaveOlderAdultResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = CreateOlderAdultCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            olderAdult => CreatedAtAction(nameof(GetOlderAdultById), new { id = olderAdult.Id },
                OlderAdultResourceFromEntityAssembler.ToResourceFromEntity(olderAdult)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Replaces the data of an older adult.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>status</c> is required; use <c>Archived</c> to end
    ///     the follow-up (there is no delete). <c>caregiverId</c> cannot change (<c>409</c>).
    /// </remarks>
    /// <param name="id">Older adult identifier.</param>
    /// <param name="resource">The whole older adult.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated older adult.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(OlderAdultResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    public async Task<ActionResult> UpdateOlderAdult(int id, [FromBody] SaveOlderAdultResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        if (resource.Status is null)
        {
            ModelState.AddModelError("status", localizer["RequiredField"].Value);
            return ValidationProblem(ModelState);
        }

        var command = UpdateOlderAdultCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            olderAdult => Ok(OlderAdultResourceFromEntityAssembler.ToResourceFromEntity(olderAdult)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }
}
