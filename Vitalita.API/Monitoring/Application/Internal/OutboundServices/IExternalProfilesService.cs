namespace Vitalita.API.Monitoring.Application.Internal.OutboundServices;

/// <summary>
///     What Monitoring needs from the Profiles context, expressed in its own terms.
/// </summary>
public interface IExternalProfilesService
{
    /// <summary>
    ///     Checks whether an older adult exists.
    /// </summary>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when it exists.</returns>
    Task<bool> ExistsOlderAdultAsync(int olderAdultId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user can read the follow-up of an older adult (caregiver in charge or active family).
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the user can read it; <c>false</c> also when the older adult does not exist.</returns>
    Task<bool> CanReadOlderAdultAsync(int userId, int olderAdultId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user can write the follow-up of an older adult (caregiver in charge only).
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the user can write it; <c>false</c> also when the older adult does not exist.</returns>
    Task<bool> CanManageOlderAdultAsync(int userId, int olderAdultId, CancellationToken cancellationToken = default);
}
