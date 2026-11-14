using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveFamilyMemberProfileResource" /> into an <see cref="UpdateFamilyMemberProfileCommand" />.
/// </summary>
public static class UpdateFamilyMemberProfileCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="id">Profile identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateFamilyMemberProfileCommand ToCommandFromResource(int id, Requester requester,
        SaveFamilyMemberProfileResource resource) =>
        new(id, requester, resource.UserId!.Value, resource.FirstName, resource.LastName, resource.PhoneNumber);
}
