namespace Vitalita.API.Profiles.Interfaces.ACL;

/// <summary>
///     Anti-corruption layer facade that other bounded contexts (Monitoring, Planning, Dashboard, ...) use to query
///     Profiles.
/// </summary>
/// <remarks>
///     <para>Other contexts depend only on this interface, never on the Profiles domain or infrastructure.</para>
///     <para>
///         The facade does not know the JWT claims: callers grant administrators access themselves
///         (<c>User.IsAdmin()</c>) before asking the facade.
///     </para>
/// </remarks>
public interface IProfilesContextFacade
{
    /// <summary>
    ///     Checks whether an older adult exists.
    /// </summary>
    /// <param name="olderAdultId">The older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the older adult exists (any status).</returns>
    Task<bool> ExistsOlderAdultByIdAsync(int olderAdultId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user can read the follow-up of an older adult.
    /// </summary>
    /// <param name="userId">The IAM user identifier.</param>
    /// <param name="olderAdultId">The older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     <c>true</c> for the caregiver in charge and for family members with an active access; <c>false</c> when
    ///     the older adult does not exist.
    /// </returns>
    Task<bool> CanUserAccessOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user can write the follow-up of an older adult (daily reports, vital signs, ...).
    /// </summary>
    /// <param name="userId">The IAM user identifier.</param>
    /// <param name="olderAdultId">The older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> only for the caregiver in charge; <c>false</c> when the older adult does not exist.</returns>
    Task<bool> CanUserManageOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the caregiver profile identifier of a user.
    /// </summary>
    /// <param name="userId">The IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The caregiver profile identifier, or <c>null</c> when the user has none.</returns>
    Task<int?> GetCaregiverProfileIdByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the IAM users of the family members with an active access to an older adult (for example, the
    ///     recipients of a notification).
    /// </summary>
    /// <param name="olderAdultId">The older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user identifiers, without duplicates; empty when there are none.</returns>
    Task<IReadOnlyList<int>> GetActiveFamilyUserIdsAsync(int olderAdultId,
        CancellationToken cancellationToken = default);
}
