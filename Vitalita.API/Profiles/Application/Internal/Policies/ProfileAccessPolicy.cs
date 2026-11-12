using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;

namespace Vitalita.API.Profiles.Application.Internal.Policies;

/// <summary>
///     Decides who can read and manage the Profiles resources.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>Administrators can read and manage everything.</description></item>
///         <item><description>A caregiver manages the older adults in their charge and the accesses to them.</description></item>
///         <item><description>A family member reads an older adult only with an <see cref="AccessStatus.Active" /> access.</description></item>
///         <item><description>A family member reads the caregiver profile of an older adult they follow.</description></item>
///         <item><description>A caregiver reads the family member profiles linked to their older adults (any access status).</description></item>
///     </list>
///     Rules are based on the requester's profiles, not on the role claim, so the ACL facade can reuse them with a
///     plain user identifier.
/// </remarks>
/// <param name="caregiverProfileRepository">Caregiver profile repository.</param>
/// <param name="familyMemberProfileRepository">Family member profile repository.</param>
/// <param name="olderAdultRepository">Older adult repository.</param>
/// <param name="familyAccessRepository">Family access repository.</param>
public class ProfileAccessPolicy(
    ICaregiverProfileRepository caregiverProfileRepository,
    IFamilyMemberProfileRepository familyMemberProfileRepository,
    IOlderAdultRepository olderAdultRepository,
    IFamilyAccessRepository familyAccessRepository)
{
    /// <summary>
    ///     Gets the caregiver profile identifier of the requester.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile identifier, or <c>null</c> when the requester has no caregiver profile.</returns>
    public async Task<int?> GetCaregiverProfileIdAsync(Requester requester,
        CancellationToken cancellationToken = default) =>
        (await caregiverProfileRepository.FindByUserIdAsync(requester.UserId, cancellationToken))?.Id;

    /// <summary>
    ///     Gets the family member profile identifier of the requester.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile identifier, or <c>null</c> when the requester has no family member profile.</returns>
    public async Task<int?> GetFamilyMemberProfileIdAsync(Requester requester,
        CancellationToken cancellationToken = default) =>
        (await familyMemberProfileRepository.FindByUserIdAsync(requester.UserId, cancellationToken))?.Id;

    /// <summary>
    ///     Checks whether the requester can change an older adult and its family accesses.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdult">The older adult.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> for administrators and the caregiver in charge.</returns>
    public async Task<bool> CanManageOlderAdultAsync(Requester requester, OlderAdult olderAdult,
        CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin) return true;
        return await GetCaregiverProfileIdAsync(requester, cancellationToken) is { } caregiverId &&
               olderAdult.IsInChargeOf(caregiverId);
    }

    /// <summary>
    ///     Checks whether the requester can change an older adult, by identifier.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdultId">The older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>false</c> when the older adult does not exist and the requester is not an administrator.</returns>
    public async Task<bool> CanManageOlderAdultAsync(Requester requester, int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin) return true;
        var olderAdult = await olderAdultRepository.FindByIdAsync(olderAdultId, cancellationToken);
        return olderAdult is not null && await CanManageOlderAdultAsync(requester, olderAdult, cancellationToken);
    }

    /// <summary>
    ///     Checks whether the requester can read an older adult.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdult">The older adult.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> for administrators, the caregiver in charge and family members with an active access.</returns>
    public async Task<bool> CanReadOlderAdultAsync(Requester requester, OlderAdult olderAdult,
        CancellationToken cancellationToken = default)
    {
        if (await CanManageOlderAdultAsync(requester, olderAdult, cancellationToken)) return true;
        return await GetFamilyMemberProfileIdAsync(requester, cancellationToken) is { } familyMemberId &&
               await familyAccessRepository.ExistsActiveAccessAsync(familyMemberId, olderAdult.Id, cancellationToken);
    }

    /// <summary>
    ///     Keeps only the older adults the requester can read.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdults">The older adults.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The readable older adults, in the same order.</returns>
    public async Task<IReadOnlyList<OlderAdult>> FilterReadableOlderAdultsAsync(Requester requester,
        IReadOnlyList<OlderAdult> olderAdults, CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin || olderAdults.Count == 0) return olderAdults;

        var caregiverId = await GetCaregiverProfileIdAsync(requester, cancellationToken);
        var followedIds = await GetActivelyFollowedOlderAdultIdsAsync(requester, cancellationToken);
        return olderAdults
            .Where(olderAdult => (caregiverId is { } id && olderAdult.IsInChargeOf(id)) ||
                                 followedIds.Contains(olderAdult.Id))
            .ToList();
    }

    /// <summary>
    ///     Checks whether the requester can read a caregiver profile.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="profile">The caregiver profile.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     <c>true</c> for administrators, the owner and family members with an active access to an older adult in
    ///     the charge of that caregiver.
    /// </returns>
    public async Task<bool> CanReadCaregiverProfileAsync(Requester requester, CaregiverProfile profile,
        CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin || profile.UserId == requester.UserId) return true;

        var followedIds = await GetActivelyFollowedOlderAdultIdsAsync(requester, cancellationToken);
        if (followedIds.Count == 0) return false;
        var followed = await olderAdultRepository.FindByIdsAsync(followedIds, cancellationToken);
        return followed.Any(olderAdult => olderAdult.IsInChargeOf(profile.Id));
    }

    /// <summary>
    ///     Keeps only the family member profiles the requester can read.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="profiles">The family member profiles.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     The profiles of the requester and, for caregivers, the profiles linked by any access to an older adult in
    ///     their charge. Administrators get every profile.
    /// </returns>
    public async Task<IReadOnlyList<FamilyMemberProfile>> FilterReadableFamilyMemberProfilesAsync(
        Requester requester, IReadOnlyList<FamilyMemberProfile> profiles,
        CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin || profiles.Count == 0) return profiles;

        var linkedIds = new HashSet<int>();
        if (await GetCaregiverProfileIdAsync(requester, cancellationToken) is { } caregiverId)
        {
            var olderAdultIds = (await olderAdultRepository.FindByCaregiverIdAsync(caregiverId, cancellationToken))
                .Select(olderAdult => olderAdult.Id).ToList();
            if (olderAdultIds.Count > 0)
            {
                var accesses = await familyAccessRepository.FindByOlderAdultIdsAsync(olderAdultIds, null,
                    cancellationToken);
                linkedIds.UnionWith(accesses.Where(access => access.FamilyMemberId is not null)
                    .Select(access => access.FamilyMemberId!.Value));
            }
        }

        return profiles.Where(profile => profile.UserId == requester.UserId || linkedIds.Contains(profile.Id))
            .ToList();
    }

    /// <summary>
    ///     Checks whether the requester can read a family access.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="access">The family access.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> for administrators, the caregiver in charge and the linked family member.</returns>
    public async Task<bool> CanReadFamilyAccessAsync(Requester requester, FamilyAccess access,
        CancellationToken cancellationToken = default)
    {
        if (await CanManageOlderAdultAsync(requester, access.OlderAdultId, cancellationToken)) return true;
        return access.FamilyMemberId is { } linkedId &&
               await GetFamilyMemberProfileIdAsync(requester, cancellationToken) == linkedId;
    }

    /// <summary>
    ///     Gets the older adults the requester follows as a family member with an active access.
    /// </summary>
    private async Task<IReadOnlyCollection<int>> GetActivelyFollowedOlderAdultIdsAsync(Requester requester,
        CancellationToken cancellationToken)
    {
        if (await GetFamilyMemberProfileIdAsync(requester, cancellationToken) is not { } familyMemberId) return [];
        var accesses = await familyAccessRepository.FindByFamilyMemberIdAsync(familyMemberId, AccessStatus.Active,
            cancellationToken);
        return accesses.Select(access => access.OlderAdultId).ToHashSet();
    }
}
