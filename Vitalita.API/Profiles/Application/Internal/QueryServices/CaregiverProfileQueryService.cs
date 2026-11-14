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
///     Application service that handles the <see cref="CaregiverProfile" /> queries.
/// </summary>
/// <param name="caregiverProfileRepository">Caregiver profile repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
public class CaregiverProfileQueryService(
    ICaregiverProfileRepository caregiverProfileRepository,
    ProfileAccessPolicy accessPolicy)
    : ICaregiverProfileQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>> Handle(
        GetAllCaregiverProfilesQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(ProfileQueryError.Forbidden);
        return ListSuccess((await caregiverProfileRepository.ListAsync(cancellationToken)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>> Handle(
        GetCaregiverProfilesByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin && query.Requester.UserId != query.UserId)
            return ListFailure(ProfileQueryError.Forbidden);

        var profile = await caregiverProfileRepository.FindByUserIdAsync(query.UserId, cancellationToken);
        return ListSuccess(profile is null ? [] : [profile]);
    }

    /// <inheritdoc />
    public async Task<Result<CaregiverProfile, ProfileQueryError>> Handle(GetCaregiverProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await caregiverProfileRepository.FindByIdAsync(query.Id, cancellationToken);
        if (profile is null) return new Result<CaregiverProfile, ProfileQueryError>.Failure(ProfileQueryError.NotFound);
        return await accessPolicy.CanReadCaregiverProfileAsync(query.Requester, profile, cancellationToken)
            ? new Result<CaregiverProfile, ProfileQueryError>.Success(profile)
            : new Result<CaregiverProfile, ProfileQueryError>.Failure(ProfileQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError> ListSuccess(
        IReadOnlyList<CaregiverProfile> profiles) =>
        new Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>.Success(profiles);

    private static Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError> ListFailure(ProfileQueryError error) =>
        new Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>.Failure(error);
}
