using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveFamilyMemberProfileResource" /> into a <see cref="CreateFamilyMemberProfileCommand" />.
/// </summary>
public static class CreateFamilyMemberProfileCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateFamilyMemberProfileCommand ToCommandFromResource(Requester requester,
        SaveFamilyMemberProfileResource resource) =>
        new(requester, resource.UserId!.Value, resource.FirstName, resource.LastName, resource.PhoneNumber);
}
