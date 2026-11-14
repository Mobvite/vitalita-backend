using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Profiles.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="CaregiverProfile" /> aggregate.
/// </summary>
public interface ICaregiverProfileRepository : IBaseRepository<CaregiverProfile>
{
    /// <summary>
    ///     Finds the profile of a user.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile, or <c>null</c>.</returns>
    Task<CaregiverProfile?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user already has a caregiver profile.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the profile exists.</returns>
    Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a caregiver profile exists.
    /// </summary>
    /// <param name="id">Profile identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the profile exists.</returns>
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
}
