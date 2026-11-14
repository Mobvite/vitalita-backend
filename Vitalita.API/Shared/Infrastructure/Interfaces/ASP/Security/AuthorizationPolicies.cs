namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

/// <summary>
///     Names of the authorization policies shared by every bounded context.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    ///     Only users with the <c>Admin</c> role.
    /// </summary>
    public const string AdminOnly = nameof(AdminOnly);

    /// <summary>
    ///     Value of the <c>role</c> claim of administrators.
    /// </summary>
    public const string AdminRole = "Admin";
}
