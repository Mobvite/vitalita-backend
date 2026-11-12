using Vitalita.API.IAM.Application.Results;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles an <see cref="AuthenticatedUserResource" /> from an <see cref="AuthenticatedUser" />.
/// </summary>
public static class AuthenticatedUserResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the sign-in result into a resource.
    /// </summary>
    /// <param name="entity">The authenticated user and token.</param>
    /// <returns>The resource read by the frontend (<c>id</c>, <c>email</c>, <c>role</c>, <c>status</c>, <c>token</c>).</returns>
    public static AuthenticatedUserResource ToResourceFromEntity(AuthenticatedUser entity) =>
        new(entity.User.Id, entity.User.Email.Value, entity.User.Role, entity.User.Status, entity.Token);
}
