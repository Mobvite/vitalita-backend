using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     User account. It never contains the password or its hash.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">Account email.</param>
/// <param name="Role">Account role.</param>
/// <param name="Status">Account status.</param>
/// <param name="CreatedAt">UTC instant when the account was created.</param>
/// <param name="LastLoginAt">UTC instant of the last sign-in, or <c>null</c>.</param>
public record UserResource(
    int Id,
    string Email,
    UserRole Role,
    UserStatus Status,
    DateTime? CreatedAt,
    DateTime? LastLoginAt);
