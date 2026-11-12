using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.ACL.Services;

/// <summary>
///     Implementation of <see cref="IProfilesContextFacade" /> on top of the Profiles repositories and access policy.
/// </summary>
/// <remarks>
///     The requester is built without a role, so administrator rules never apply here; callers handle
///     administrators themselves.
/// </remarks>
/// <param name="olderAdultRepository">Older adult repository.</param>
/// <param name="caregiverProfileRepository">Caregiver profile repository.</param>
/// <param name="familyMemberProfileRepository">Family member profile repository.</param>
/// <param name="familyAccessRepository">Family access repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
public class ProfilesContextFacade(
    IOlderAdultRepository olderAdultRepository,
    ICaregiverProfileRepository caregiverProfileRepository,
    IFamilyMemberProfileRepository familyMemberProfileRepository,
    IFamilyAccessRepository familyAccessRepository,
    ProfileAccessPolicy accessPolicy)
    : IProfilesContextFacade
{
    /// <inheritdoc />
    public async Task<bool> ExistsOlderAdultByIdAsync(int olderAdultId, CancellationToken cancellationToken = default)
    {
        if (olderAdultId <= 0) return false;
        return await olderAdultRepository.FindByIdAsync(olderAdultId, cancellationToken) is not null;
    }

    /// <inheritdoc />
    public async Task<bool> CanUserAccessOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || olderAdultId <= 0) return false;
        var olderAdult = await olderAdultRepository.FindByIdAsync(olderAdultId, cancellationToken);
        return olderAdult is not null &&
               await accessPolicy.CanReadOlderAdultAsync(AsRequester(userId), olderAdult, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CanUserManageOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || olderAdultId <= 0) return false;
        return await accessPolicy.CanManageOlderAdultAsync(AsRequester(userId), olderAdultId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int?> GetCaregiverProfileIdByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return null;
        return (await caregiverProfileRepository.FindByUserIdAsync(userId, cancellationToken))?.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetActiveFamilyUserIdsAsync(int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        if (olderAdultId <= 0) return [];
        var accesses = await familyAccessRepository.FindByOlderAdultIdsAsync([olderAdultId], AccessStatus.Active,
            cancellationToken);
        var profileIds = accesses.Where(access => access.FamilyMemberId is not null)
            .Select(access => access.FamilyMemberId!.Value).Distinct().ToList();
        if (profileIds.Count == 0) return [];

        var profiles = await familyMemberProfileRepository.FindByIdsAsync(profileIds, cancellationToken);
        return profiles.Select(profile => profile.UserId).Distinct().ToList();
    }

    private static Requester AsRequester(int userId) => new(userId, null, null);
}
