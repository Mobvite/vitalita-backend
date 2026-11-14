namespace Vitalita.API.IAM.Interfaces.ACL;

/// <summary>
///     Anti-corruption layer facade that other bounded contexts use to query IAM.
/// </summary>
/// <remarks>
///     Other contexts depend only on this interface, never on the IAM domain or infrastructure.
/// </remarks>
public interface IIamContextFacade
{
    /// <summary>
    ///     Checks whether a user exists.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when the user exists (active or inactive).</returns>
    Task<bool> ExistsUserByIdAsync(int userId, CancellationToken cancellationToken = default);
}
