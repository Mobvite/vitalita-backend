using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.IAM.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="User" /> aggregate.
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    ///     Finds a user by email.
    /// </summary>
    /// <param name="email">The normalized email.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The user if found; otherwise <c>null</c>.</returns>
    Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user with the given email exists.
    /// </summary>
    /// <param name="email">The normalized email.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the email is already registered.</returns>
    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether a user with the given identifier exists.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the user exists.</returns>
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
}
