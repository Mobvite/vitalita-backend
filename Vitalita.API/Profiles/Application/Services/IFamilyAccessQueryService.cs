using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Query service of the <see cref="FamilyAccess" /> aggregate.
/// </summary>
public interface IFamilyAccessQueryService
{
    /// <summary>
    ///     Lists every family access (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, or Forbidden.</returns>
    Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(GetAllFamilyAccessesQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the accesses of an older adult.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, or Forbidden when the requester is not the caregiver in charge.</returns>
    Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(GetFamilyAccessesByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the accesses of a family member.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, or Forbidden when the profile is not the requester's.</returns>
    Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(GetFamilyAccessesByFamilyMemberIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a family access by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The access, NotFound or Forbidden.</returns>
    Task<Result<FamilyAccess, ProfileQueryError>> Handle(GetFamilyAccessByIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Looks for an invitation that can still be accepted (public).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>Zero or one pending, unexpired invitation.</returns>
    Task<IReadOnlyList<FamilyAccess>> Handle(GetValidInvitationByCodeQuery query,
        CancellationToken cancellationToken = default);
}
