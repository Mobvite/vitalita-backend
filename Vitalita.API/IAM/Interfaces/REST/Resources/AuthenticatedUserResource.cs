using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Response of a successful sign-in.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account email.</param>
/// <param name="Role">Account role.</param>
/// <param name="Status">Account status.</param>
/// <param name="Token">JWT access token, sent as <c>Authorization: Bearer &lt;token&gt;</c>.</param>
public record AuthenticatedUserResource(int Id, string Email, UserRole Role, UserStatus Status, string Token);
