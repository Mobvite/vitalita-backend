using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles a <see cref="UserResource" /> from a <see cref="User" />. The password hash is never copied.
/// </summary>
public static class UserResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into a resource.
    /// </summary>
    /// <param name="entity">The user.</param>
    /// <returns>The user resource.</returns>
    public static UserResource ToResourceFromEntity(User entity) =>
        new(entity.Id, entity.Email.Value, entity.Role, entity.Status, entity.CreatedAt, entity.LastLoginAt);
}
