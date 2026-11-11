namespace Vitalita.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to get a user by identifier.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public record GetUserByIdQuery(int UserId);
