namespace Vitalita.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to get the user of the access token, checking that it still exists and is active.
/// </summary>
/// <param name="UserId">The user identifier (claim <c>sub</c> of the token).</param>
public record GetCurrentUserQuery(int UserId);
