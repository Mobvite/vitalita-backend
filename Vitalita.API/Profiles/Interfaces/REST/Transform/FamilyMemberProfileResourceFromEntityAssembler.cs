using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="FamilyMemberProfile" /> into a <see cref="FamilyMemberProfileResource" />.
/// </summary>
public static class FamilyMemberProfileResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource.
    /// </summary>
    /// <param name="entity">The family member profile.</param>
    /// <returns>The resource.</returns>
    public static FamilyMemberProfileResource ToResourceFromEntity(FamilyMemberProfile entity) =>
        new(entity.Id, entity.UserId, entity.FirstName, entity.LastName, entity.PhoneNumber);
}
