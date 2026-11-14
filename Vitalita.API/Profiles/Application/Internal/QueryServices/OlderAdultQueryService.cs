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
///     Application service that handles the <see cref="OlderAdult" /> queries.
/// </summary>
/// <param name="olderAdultRepository">Older adult repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
public class OlderAdultQueryService(
    IOlderAdultRepository olderAdultRepository,
    ProfileAccessPolicy accessPolicy)
    : IOlderAdultQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<OlderAdult>, ProfileQueryError>> Handle(GetAllOlderAdultsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(ProfileQueryError.Forbidden);
        return ListSuccess((await olderAdultRepository.ListAsync(cancellationToken)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<OlderAdult>, ProfileQueryError>> Handle(
        GetOlderAdultsByCaregiverIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin &&
            await accessPolicy.GetCaregiverProfileIdAsync(query.Requester, cancellationToken) != query.CaregiverId)
            return ListFailure(ProfileQueryError.Forbidden);

        return ListSuccess(await olderAdultRepository.FindByCaregiverIdAsync(query.CaregiverId, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OlderAdult>> Handle(GetOlderAdultsByIdsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Ids.Count == 0) return [];
        var olderAdults = await olderAdultRepository.FindByIdsAsync(query.Ids, cancellationToken);
        return await accessPolicy.FilterReadableOlderAdultsAsync(query.Requester, olderAdults, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<OlderAdult, ProfileQueryError>> Handle(GetOlderAdultByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var olderAdult = await olderAdultRepository.FindByIdAsync(query.Id, cancellationToken);
        if (olderAdult is null) return new Result<OlderAdult, ProfileQueryError>.Failure(ProfileQueryError.NotFound);
        return await accessPolicy.CanReadOlderAdultAsync(query.Requester, olderAdult, cancellationToken)
            ? new Result<OlderAdult, ProfileQueryError>.Success(olderAdult)
            : new Result<OlderAdult, ProfileQueryError>.Failure(ProfileQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<OlderAdult>, ProfileQueryError> ListSuccess(
        IReadOnlyList<OlderAdult> olderAdults) =>
        new Result<IReadOnlyList<OlderAdult>, ProfileQueryError>.Success(olderAdults);

    private static Result<IReadOnlyList<OlderAdult>, ProfileQueryError> ListFailure(ProfileQueryError error) =>
        new Result<IReadOnlyList<OlderAdult>, ProfileQueryError>.Failure(error);
}
