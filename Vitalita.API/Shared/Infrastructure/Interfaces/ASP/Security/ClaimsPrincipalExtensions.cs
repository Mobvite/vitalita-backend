using System.Globalization;
using System.Security.Claims;

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
}
