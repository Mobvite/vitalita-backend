namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Minimal user data returned by the public email lookup (<c>GET /users?email=</c>).
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account email.</param>
public record UserEmailResource(int Id, string Email);
