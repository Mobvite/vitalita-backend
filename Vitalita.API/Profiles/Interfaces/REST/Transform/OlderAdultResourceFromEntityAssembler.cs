using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts an <see cref="OlderAdult" /> into an <see cref="OlderAdultResource" />.
/// </summary>
public static class OlderAdultResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource. An unknown emergency contact relationship is an empty string,
    ///     as the frontend expects.
    /// </summary>
    /// <param name="entity">The older adult.</param>
    /// <returns>The resource.</returns>
    public static OlderAdultResource ToResourceFromEntity(OlderAdult entity) =>
        new(entity.Id, entity.CaregiverId, entity.FirstName, entity.LastName, entity.BirthDate,
            entity.DocumentNumber, entity.BloodType, [..entity.Allergies], [..entity.ChronicConditions],
            new EmergencyContactResource(entity.EmergencyContact.Name, entity.EmergencyContact.PhoneNumber,
                entity.EmergencyContact.Relationship?.ToString() ?? string.Empty),
            entity.Observations, entity.Status);
}
