using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="FamilyMemberProfile" /> queries.
/// </summary>
/// <param name="familyMemberProfileRepository">Family member profile repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
public class FamilyMemberProfileQueryService(
    IFamilyMemberProfileRepository familyMemberProfileRepository,
    ProfileAccessPolicy accessPolicy)
    : IFamilyMemberProfileQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>> Handle(
        GetAllFamilyMemberProfilesQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(ProfileQueryError.Forbidden);
        return ListSuccess((await familyMemberProfileRepository.ListAsync(cancellationToken)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>> Handle(
        GetFamilyMemberProfilesByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin && query.Requester.UserId != query.UserId)
            return ListFailure(ProfileQueryError.Forbidden);

        var profile = await familyMemberProfileRepository.FindByUserIdAsync(query.UserId, cancellationToken);
        return ListSuccess(profile is null ? [] : [profile]);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyMemberProfile>> Handle(GetFamilyMemberProfilesByIdsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Ids.Count == 0) return [];
        var profiles = await familyMemberProfileRepository.FindByIdsAsync(query.Ids, cancellationToken);
        return await accessPolicy.FilterReadableFamilyMemberProfilesAsync(query.Requester, profiles,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<FamilyMemberProfile, ProfileQueryError>> Handle(GetFamilyMemberProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await familyMemberProfileRepository.FindByIdAsync(query.Id, cancellationToken);
        if (profile is null)
            return new Result<FamilyMemberProfile, ProfileQueryError>.Failure(ProfileQueryError.NotFound);

        var readable = await accessPolicy.FilterReadableFamilyMemberProfilesAsync(query.Requester, [profile],
            cancellationToken);
        return readable.Count == 1
            ? new Result<FamilyMemberProfile, ProfileQueryError>.Success(profile)
            : new Result<FamilyMemberProfile, ProfileQueryError>.Failure(ProfileQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError> ListSuccess(
        IReadOnlyList<FamilyMemberProfile> profiles) =>
        new Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>.Success(profiles);

    private static Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError> ListFailure(
        ProfileQueryError error) =>
        new Result<IReadOnlyList<FamilyMemberProfile>, ProfileQueryError>.Failure(error);
}
