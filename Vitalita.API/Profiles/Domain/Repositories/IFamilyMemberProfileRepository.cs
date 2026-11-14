using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Profiles.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="FamilyMemberProfile" /> aggregate.
/// </summary>
public interface IFamilyMemberProfileRepository : IBaseRepository<FamilyMemberProfile>
{
    /// <summary>
    ///     Finds the profile of a user.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile, or <c>null</c>.</returns>
    Task<FamilyMemberProfile?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user already has a family member profile.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the profile exists.</returns>
    Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds several profiles by identifier.
    /// </summary>
    /// <param name="ids">Profile identifiers.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profiles found, ordered by identifier.</returns>
    Task<IReadOnlyList<FamilyMemberProfile>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);
}
