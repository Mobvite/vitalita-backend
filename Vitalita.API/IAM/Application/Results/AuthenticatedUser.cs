using Vitalita.API.IAM.Domain.Model.Aggregates;

namespace Vitalita.API.IAM.Application.Results;

/// <summary>
///     Result of a successful sign-in.
/// </summary>
/// <param name="User">The authenticated user.</param>
/// <param name="Token">The access token.</param>
public record AuthenticatedUser(User User, string Token);
