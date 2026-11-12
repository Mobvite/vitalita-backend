namespace Vitalita.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Password hashing port. The implementation lives in the infrastructure layer (BCrypt).
/// </summary>
public interface IHashingService
{
    /// <summary>
    ///     Hashes a plain password.
    /// </summary>
    /// <param name="password">The plain password.</param>
    /// <returns>The password hash.</returns>
    string HashPassword(string password);

    /// <summary>
    ///     Verifies a plain password against a hash.
    /// </summary>
    /// <remarks>
    ///     When <paramref name="passwordHash" /> is <c>null</c> (unknown email), the implementation still spends the
    ///     same time verifying against a dummy hash and returns <c>false</c>, so the response time does not reveal
    ///     whether the email exists.
    /// </remarks>
    /// <param name="password">The plain password.</param>
    /// <param name="passwordHash">The stored hash, or <c>null</c> when there is no user.</param>
    /// <returns><c>true</c> when the password matches the hash.</returns>
    bool VerifyPassword(string password, string? passwordHash);
}
