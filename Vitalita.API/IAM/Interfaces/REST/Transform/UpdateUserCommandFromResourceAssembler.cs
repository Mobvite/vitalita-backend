using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles an <see cref="UpdateUserCommand" /> from an <see cref="UpdateUserResource" />.
/// </summary>
public static class UpdateUserCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the resource into a command.
    /// </summary>
    /// <param name="userId">Identifier of the user to update (route).</param>
    /// <param name="requesterId">Identifier of the authenticated user.</param>
    /// <param name="requesterIsAdmin">Whether the authenticated user is an administrator.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The update user command.</returns>
    public static UpdateUserCommand ToCommandFromResource(int userId, int requesterId, bool requesterIsAdmin,
        UpdateUserResource resource) =>
        new(userId, requesterId, requesterIsAdmin,
            new EmailAddress(resource.Email),
            resource.Password,
            Enum.Parse<UserRole>(resource.Role),
            Enum.Parse<UserStatus>(resource.Status));
}
