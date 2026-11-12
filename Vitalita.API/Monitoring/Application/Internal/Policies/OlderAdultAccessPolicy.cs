using Vitalita.API.Monitoring.Application.Internal.OutboundServices;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Application.Internal.Policies;

/// <summary>
///     Who can read and write the monitoring records of an older adult.
/// </summary>
/// <remarks>
///     Administrators can do everything. Otherwise reads need the caregiver in charge or a family member with an
///     active access, and writes need the caregiver in charge (both answered by the Profiles context).
/// </remarks>
/// <param name="profilesService">Outbound service to the Profiles context.</param>
public class OlderAdultAccessPolicy(IExternalProfilesService profilesService)
{
    /// <summary>
    ///     Checks whether the requester can read the records of an older adult.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> for administrators, the caregiver in charge and family members with an active access.</returns>
    public async Task<bool> CanReadAsync(Requester requester, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        requester.IsAdmin ||
        await profilesService.CanReadOlderAdultAsync(requester.UserId, olderAdultId, cancellationToken);

    /// <summary>
    ///     Checks whether the requester can write the records of an older adult.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> for administrators and the caregiver in charge.</returns>
    public async Task<bool> CanWriteAsync(Requester requester, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        requester.IsAdmin ||
        await profilesService.CanManageOlderAdultAsync(requester.UserId, olderAdultId, cancellationToken);

    /// <summary>
    ///     Checks whether the requester can add a record to an older adult.
    /// </summary>
    /// <remarks>
    ///     Administrators get <see cref="NewRecordAccess.OlderAdultNotFound" /> when the older adult does not exist;
    ///     other users get <see cref="NewRecordAccess.Forbidden" />, so they cannot discover which identifiers exist.
    /// </remarks>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>Whether the record can be added.</returns>
    public async Task<NewRecordAccess> CheckNewRecordAsync(Requester requester, int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        if (requester.IsAdmin)
            return await profilesService.ExistsOlderAdultAsync(olderAdultId, cancellationToken)
                ? NewRecordAccess.Allowed
                : NewRecordAccess.OlderAdultNotFound;

        return await profilesService.CanManageOlderAdultAsync(requester.UserId, olderAdultId, cancellationToken)
            ? NewRecordAccess.Allowed
            : NewRecordAccess.Forbidden;
    }
}

/// <summary>
///     Result of <see cref="OlderAdultAccessPolicy.CheckNewRecordAsync" />.
/// </summary>
public enum NewRecordAccess
{
    /// <summary>The record can be added.</summary>
    Allowed,

    /// <summary>The requester is not the caregiver in charge (or the older adult does not exist).</summary>
    Forbidden,

    /// <summary>The older adult does not exist (administrators only).</summary>
    OlderAdultNotFound
}
