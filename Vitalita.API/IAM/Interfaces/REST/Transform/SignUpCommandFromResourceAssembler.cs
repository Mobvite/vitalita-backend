using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles a <see cref="SignUpCommand" /> from a <see cref="SignUpResource" />.
/// </summary>
public static class SignUpCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the resource into a command. A missing role becomes <see cref="UserRole.Caregiver" />.
    /// </summary>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The sign-up command.</returns>
    public static SignUpCommand ToCommandFromResource(SignUpResource resource) =>
        new(new EmailAddress(resource.Email),
            resource.Password,
            resource.Role is null ? UserRole.Caregiver : Enum.Parse<UserRole>(resource.Role));
}
