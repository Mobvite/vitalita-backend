using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Profiles.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="FamilyAccess" /> queries.
/// </summary>
/// <param name="familyAccessRepository">Family access repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
/// <param name="clock">Clock used to discard expired invitations.</param>
public class FamilyAccessQueryService(
    IFamilyAccessRepository familyAccessRepository,
    ProfileAccessPolicy accessPolicy,
    IClock clock)
    : IFamilyAccessQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(GetAllFamilyAccessesQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(ProfileQueryError.Forbidden);
        return ListSuccess(await familyAccessRepository.FindAllAsync(query.Status, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(
        GetFamilyAccessesByOlderAdultIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!await accessPolicy.CanManageOlderAdultAsync(query.Requester, query.OlderAdultId, cancellationToken))
            return ListFailure(ProfileQueryError.Forbidden);

        return ListSuccess(await familyAccessRepository.FindByOlderAdultIdsAsync([query.OlderAdultId], query.Status,
            cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>> Handle(
        GetFamilyAccessesByFamilyMemberIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin &&
            await accessPolicy.GetFamilyMemberProfileIdAsync(query.Requester, cancellationToken) !=
            query.FamilyMemberId)
            return ListFailure(ProfileQueryError.Forbidden);

        return ListSuccess(await familyAccessRepository.FindByFamilyMemberIdAsync(query.FamilyMemberId, query.Status,
            cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<FamilyAccess, ProfileQueryError>> Handle(GetFamilyAccessByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var access = await familyAccessRepository.FindByIdAsync(query.Id, cancellationToken);
        if (access is null) return new Result<FamilyAccess, ProfileQueryError>.Failure(ProfileQueryError.NotFound);
        return await accessPolicy.CanReadFamilyAccessAsync(query.Requester, access, cancellationToken)
            ? new Result<FamilyAccess, ProfileQueryError>.Success(access)
            : new Result<FamilyAccess, ProfileQueryError>.Failure(ProfileQueryError.Forbidden);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyAccess>> Handle(GetValidInvitationByCodeQuery query,
        CancellationToken cancellationToken = default)
    {
        var access = await familyAccessRepository.FindByInvitationCodeAsync(query.Code, cancellationToken);
        return access is not null && access.CanBeAccepted(clock.UtcNow) ? [access] : [];
    }

    private static Result<IReadOnlyList<FamilyAccess>, ProfileQueryError> ListSuccess(
        IReadOnlyList<FamilyAccess> accesses) =>
        new Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>.Success(accesses);

    private static Result<IReadOnlyList<FamilyAccess>, ProfileQueryError> ListFailure(ProfileQueryError error) =>
        new Result<IReadOnlyList<FamilyAccess>, ProfileQueryError>.Failure(error);
}
