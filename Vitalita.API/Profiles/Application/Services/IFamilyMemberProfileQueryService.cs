using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Query service of the <see cref="FamilyMemberProfile" /> aggregate.
/// </summary>
public interface IFamilyMemberProfileQueryService
{
    /// <summary>
    ///     Lists every family member profile (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profiles, or Forbidden.</returns>
    Task<Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>> Handle(GetAllFamilyMemberProfilesQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the family member profile of a user (zero or one element).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profiles, or Forbidden when the user is another one.</returns>
    Task<Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>> Handle(GetFamilyMemberProfilesByUserIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets several family member profiles, leaving out the ones the requester cannot read.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The readable profiles.</returns>
    Task<IReadOnlyList<FamilyMemberProfile>> Handle(GetFamilyMemberProfilesByIdsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a family member profile by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile, NotFound or Forbidden.</returns>
    Task<Result<FamilyMemberProfile, ProfileQueryError>> Handle(GetFamilyMemberProfileByIdQuery query,
        CancellationToken cancellationToken = default);
}
