using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles a <see cref="UserEmailResource" /> (id and email only) from a <see cref="User" />.
/// </summary>
public static class UserEmailResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into the minimal lookup resource.
    /// </summary>
    /// <param name="entity">The user.</param>
    /// <returns>The minimal resource.</returns>
    public static UserEmailResource ToResourceFromEntity(User entity) => new(entity.Id, entity.Email.Value);
}
