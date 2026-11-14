using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts an <see cref="UpdateFamilyAccessResource" /> into an <see cref="UpdateFamilyAccessCommand" />.
/// </summary>
public static class UpdateFamilyAccessCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="id">Family access identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateFamilyAccessCommand ToCommandFromResource(int id, Requester requester,
        UpdateFamilyAccessResource resource) =>
        new(id, requester, Enum.Parse<AccessStatus>(resource.Status), resource.FamilyMemberId,
            Enum.TryParse<FamilyRelationship>(resource.Relationship, out var relationship) ? relationship : null);
}
