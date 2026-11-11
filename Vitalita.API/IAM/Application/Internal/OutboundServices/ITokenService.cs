using Vitalita.API.IAM.Domain.Model.Aggregates;

namespace Vitalita.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Access token port. The implementation lives in the infrastructure layer (JWT).
/// </summary>
public interface ITokenService
{
    /// <summary>
    ///     Generates a signed access token for a user, with the id, email and role claims.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <returns>The encoded token.</returns>
    string GenerateToken(User user);
}
