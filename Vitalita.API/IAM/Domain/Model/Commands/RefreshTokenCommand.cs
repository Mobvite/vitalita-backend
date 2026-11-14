namespace Vitalita.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to issue a new access token for the user of a valid token.
/// </summary>
/// <param name="UserId">The user identifier (claim <c>sub</c> of the current token).</param>
public record RefreshTokenCommand(int UserId);
