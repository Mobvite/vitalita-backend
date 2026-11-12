using System.Security.Claims;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Builds the <see cref="Requester" /> of a request from the JWT claims.
/// </summary>
public static class RequesterFromClaimsAssembler
{
    /// <summary>
    ///     Converts the authenticated user into a <see cref="Requester" />.
    /// </summary>
    /// <param name="principal">The current user.</param>
    /// <returns>The requester, or <c>null</c> when there is no valid <c>sub</c> claim.</returns>
    public static Requester? ToRequesterFromPrincipal(ClaimsPrincipal principal) =>
        principal.GetUserId() is { } userId ? new Requester(userId, principal.GetRole(), principal.GetEmail()) : null;
}
