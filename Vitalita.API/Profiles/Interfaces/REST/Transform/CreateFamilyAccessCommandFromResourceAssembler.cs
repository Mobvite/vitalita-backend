using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="CreateFamilyAccessResource" /> into a <see cref="CreateFamilyAccessCommand" />.
/// </summary>
public static class CreateFamilyAccessCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateFamilyAccessCommand ToCommandFromResource(Requester requester,
        CreateFamilyAccessResource resource) =>
        new(requester, resource.OlderAdultId!.Value, resource.InvitedEmail,
            Enum.Parse<FamilyRelationship>(resource.Relationship));
}
