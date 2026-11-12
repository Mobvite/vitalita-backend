using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="CaregiverProfile" /> into a <see cref="CaregiverProfileResource" />.
/// </summary>
public static class CaregiverProfileResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource.
    /// </summary>
    /// <param name="entity">The caregiver profile.</param>
    /// <returns>The resource.</returns>
    public static CaregiverProfileResource ToResourceFromEntity(CaregiverProfile entity) =>
        new(entity.Id, entity.UserId, entity.FirstName, entity.LastName, entity.PhoneNumber,
            entity.ProfessionalTitle, entity.Bio, entity.PhotoUrl, [..entity.Certifications]);
}
