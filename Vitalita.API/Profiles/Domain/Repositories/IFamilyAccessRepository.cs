using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Profiles.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="FamilyAccess" /> aggregate.
/// </summary>
public interface IFamilyAccessRepository : IBaseRepository<FamilyAccess>
{
    /// <summary>
    ///     Lists the accesses of several older adults.
    /// </summary>
    /// <param name="olderAdultIds">Older adult identifiers.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, ordered by identifier.</returns>
    Task<IReadOnlyList<FamilyAccess>> FindByOlderAdultIdsAsync(IReadOnlyCollection<int> olderAdultIds,
        AccessStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the accesses of a family member.
    /// </summary>
    /// <param name="familyMemberId">Family member profile identifier.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, ordered by identifier.</returns>
    Task<IReadOnlyList<FamilyAccess>> FindByFamilyMemberIdAsync(int familyMemberId, AccessStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every access, optionally filtered by status.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The accesses, ordered by identifier.</returns>
    Task<IReadOnlyList<FamilyAccess>> FindAllAsync(AccessStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an access by invitation code.
    /// </summary>
    /// <param name="code">The invitation code.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The access, or <c>null</c>.</returns>
    Task<FamilyAccess?> FindByInvitationCodeAsync(InvitationCode code, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an invitation code is already used.
    /// </summary>
    /// <param name="code">The invitation code.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when another access has the code.</returns>
    Task<bool> ExistsByInvitationCodeAsync(InvitationCode code, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an older adult already has a pending or active access for an email.
    /// </summary>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="invitedEmail">The normalized email.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when a non-revoked access exists.</returns>
    Task<bool> ExistsOpenAccessAsync(int olderAdultId, string invitedEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether an email has an invitation that can still be accepted.
    /// </summary>
    /// <param name="invitedEmail">The normalized email.</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when a pending, unexpired invitation exists.</returns>
    Task<bool> ExistsValidInvitationForEmailAsync(string invitedEmail, DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a family member has an active access to an older adult.
    /// </summary>
    /// <param name="familyMemberId">Family member profile identifier.</param>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the access is active.</returns>
    Task<bool> ExistsActiveAccessAsync(int familyMemberId, int olderAdultId,
        CancellationToken cancellationToken = default);
}
