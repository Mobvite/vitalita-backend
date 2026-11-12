using System.Globalization;
using System.Security.Claims;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

/// <summary>
///     Helpers to read the authenticated user from the JWT claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    ///     Name of the claim that carries the user identifier.
    /// </summary>
    public const string UserIdClaimType = "sub";

    /// <summary>
    ///     Name of the claim that carries the user email.
    /// </summary>
    public const string EmailClaimType = "email";

    /// <summary>
    ///     Name of the claim that carries the user role.
    /// </summary>
    public const string RoleClaimType = "role";

    /// <summary>
    ///     Gets the identifier of the authenticated user.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns>The user identifier, or <c>null</c> when the request is anonymous or the claim is not valid.</returns>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(UserIdClaimType)?.Value;
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
    }

    /// <summary>
    ///     Checks whether the authenticated user is an administrator.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns><c>true</c> when the user has the <c>Admin</c> role.</returns>
    public static bool IsAdmin(this ClaimsPrincipal principal) => principal.IsInRole(AuthorizationPolicies.AdminRole);

    /// <summary>
    ///     Gets the email of the authenticated user.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns>The email claim, or <c>null</c> when the request is anonymous or the claim is missing.</returns>
    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal.FindFirst(EmailClaimType)?.Value is { Length: > 0 } email ? email : null;

    /// <summary>
    ///     Gets the role of the authenticated user.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns>The role claim (<c>Caregiver</c>, <c>FamilyMember</c> or <c>Admin</c>), or <c>null</c>.</returns>
    public static string? GetRole(this ClaimsPrincipal principal) =>
        principal.FindFirst(RoleClaimType)?.Value is { Length: > 0 } role ? role : null;

    /// <summary>
    ///     Builds the <see cref="Requester" /> of a request from the claims.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns>The requester, or <c>null</c> when there is no valid <c>sub</c> claim.</returns>
    public static Requester? ToRequester(this ClaimsPrincipal principal) =>
        principal.GetUserId() is { } userId ? new Requester(userId, principal.GetRole(), principal.GetEmail()) : null;
}
