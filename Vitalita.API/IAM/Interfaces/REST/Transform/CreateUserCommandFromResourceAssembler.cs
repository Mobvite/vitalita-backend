using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles a <see cref="CreateUserCommand" /> from a <see cref="CreateUserResource" />.
/// </summary>
public static class CreateUserCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the resource into a command.
    /// </summary>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The create user command.</returns>
    public static CreateUserCommand ToCommandFromResource(CreateUserResource resource) =>
        new(new EmailAddress(resource.Email), resource.Password, Enum.Parse<UserRole>(resource.Role));
}
