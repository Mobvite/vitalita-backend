using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Profiles.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Profiles.Interfaces.REST;

/// <summary>
///     Family accesses: invitations and read access of family members to older adults.
/// </summary>
/// <remarks>The route is singular (<c>/family-access</c>) to match the frontend; the table is <c>family_accesses</c>.</remarks>
/// <param name="commandService">The family access command service.</param>
/// <param name="queryService">The family access query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/family-access")]
[Tags("Family Access")]
public class FamilyAccessController(
    IFamilyAccessCommandService commandService,
    IFamilyAccessQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "FamilyAccessNotFound";

    /// <summary>
    ///     Looks up an invitation by code (public), or lists family accesses (token required).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?invitationCode=</c>: public (no token). Returns an array with the invitation only when it is
    ///         pending and not expired, with the minimal fields the sign-up form needs; otherwise <c>[]</c> (it does
    ///         not say whether the code exists, expired or was revoked). Case-insensitive.
    ///     </para>
    ///     <para><c>?olderAdultId=</c>: the accesses of an older adult; caregiver in charge only.</para>
    ///     <para><c>?familyMemberId=</c>: the accesses of the requester's own family member profile.</para>
    ///     <para>Without filters: every access, administrators only. <c>status</c> filters all the token cases.</para>
    ///     <para>Precedence: <c>invitationCode</c>, then <c>olderAdultId</c>, then <c>familyMemberId</c>.</para>
    /// </remarks>
    /// <param name="invitationCode">Invitation code (public lookup).</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="familyMemberId">Family member profile identifier.</param>
    /// <param name="status">Pending, Active or Revoked.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The invitation view for the public lookup, or the full accesses (plain arrays).</returns>
    [HttpGet]
    [AllowAnonymous]
    [OptionalBearerAuthentication]
    [ProducesResponseType(typeof(IEnumerable<FamilyAccessResource>), StatusCodes.Status200OK,
        MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetFamilyAccesses(
        [FromQuery] [StringLength(InvitationCode.MaxLength, ErrorMessage = "InvalidInvitationCode")]
        string? invitationCode,
        [FromQuery] int? olderAdultId,
        [FromQuery] int? familyMemberId,
        [FromQuery] [AllowedValues(null, nameof(AccessStatus.Pending), nameof(AccessStatus.Active),
            nameof(AccessStatus.Revoked), ErrorMessage = "InvalidAccessStatus")]
        string? status,
        CancellationToken cancellationToken)
    {
        if (Request.Query.ContainsKey(nameof(invitationCode)))
        {
            if (!InvitationCode.IsValid(invitationCode))
            {
                ModelState.AddModelError(nameof(invitationCode), localizer["InvalidInvitationCode"].Value);
                return ValidationProblem(ModelState);
            }

            var invitations = await queryService.Handle(
                new GetValidInvitationByCodeQuery(new InvitationCode(invitationCode!)), cancellationToken);
            return Ok(invitations.Select(FamilyAccessResourceFromEntityAssembler.ToInvitationResourceFromEntity));
        }

        if (User.Identity?.IsAuthenticated != true ||
            RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester)
            return Challenge();

        AccessStatus? statusFilter = Enum.TryParse<AccessStatus>(status, out var parsed) ? parsed : null;
        var result = (olderAdultId, familyMemberId) switch
        {
            ({ } olderAdult, _) => await queryService.Handle(
                new GetFamilyAccessesByOlderAdultIdQuery(requester, olderAdult, statusFilter), cancellationToken),
            (null, { } familyMember) => await queryService.Handle(
                new GetFamilyAccessesByFamilyMemberIdQuery(requester, familyMember, statusFilter), cancellationToken),
            _ => await queryService.Handle(new GetAllFamilyAccessesQuery(requester, statusFilter), cancellationToken)
        };
        return result.Fold<ActionResult>(
            accesses => Ok(accesses.Select(FamilyAccessResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a family access by identifier.
    /// </summary>
    /// <remarks>The caregiver in charge, the linked family member and administrators can read it.</remarks>
    /// <param name="id">Family access identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The family access.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FamilyAccessResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetFamilyAccessById(int id, CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetFamilyAccessByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            access => Ok(FamilyAccessResourceFromEntityAssembler.ToResourceFromEntity(access)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Invites a family member to follow an older adult.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). The server generates a cryptographically random code
    ///     (<c>VITA-</c> + 8 characters), sets <c>status</c> = <c>Pending</c>, <c>invitedAt</c> = now and
    ///     <c>expiresAt</c> = now + 7 days, and ignores those fields in the body. <c>409</c> when the older adult
    ///     already has a pending or active access for that email. The family limit of the plan is not checked yet.
    /// </remarks>
    /// <param name="resource">Older adult, invited email and relationship.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The pending access, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FamilyAccessResource), StatusCodes.Status201Created,
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
    public async Task<ActionResult> CreateFamilyAccess([FromBody] CreateFamilyAccessResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = CreateFamilyAccessCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            access => CreatedAtAction(nameof(GetFamilyAccessById), new { id = access.Id },
                FamilyAccessResourceFromEntityAssembler.ToResourceFromEntity(access)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Accepts, revokes or changes the relationship of a family access.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item><description>Pending → Active (accept): the invited person (token email = <c>invitedEmail</c>) with their own <c>familyMemberId</c>, before <c>expiresAt</c>. <c>acceptedAt</c> is set by the server.</description></item>
    ///         <item><description>→ Revoked: the caregiver in charge.</description></item>
    ///         <item><description>Same status: only <c>relationship</c> changes (caregiver in charge).</description></item>
    ///         <item><description>Revoked is final and Active cannot go back to Pending (<c>409</c>).</description></item>
    ///     </list>
    ///     Only <c>status</c>, <c>familyMemberId</c> and <c>relationship</c> are read; the other fields are ignored.
    /// </remarks>
    /// <param name="id">Family access identifier.</param>
    /// <param name="resource">The whole family access.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated family access.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FamilyAccessResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    public async Task<ActionResult> UpdateFamilyAccess(int id, [FromBody] UpdateFamilyAccessResource resource,
        CancellationToken cancellationToken)
    {
        if (RequesterFromClaimsAssembler.ToRequesterFromPrincipal(User) is not { } requester) return Challenge();

        var command = UpdateFamilyAccessCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            access => Ok(FamilyAccessResourceFromEntityAssembler.ToResourceFromEntity(access)),
            error => ActionResultFromProfilesErrorAssembler.ToActionResult(error, this, localizer));
    }
}
