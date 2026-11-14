using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Interfaces.REST.Resources;

namespace Vitalita.API.IAM.Interfaces.REST.Transform;

/// <summary>
///     Assembles a <see cref="SignInCommand" /> from a <see cref="SignInResource" />.
/// </summary>
public static class SignInCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the resource into a command.
    /// </summary>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The sign-in command.</returns>
    public static SignInCommand ToCommandFromResource(SignInResource resource) =>
        new(new EmailAddress(resource.Email), resource.Password);
}
