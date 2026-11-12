using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="Medication" /> into a <see cref="MedicationResource" />.
/// </summary>
public static class MedicationResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource.
    /// </summary>
    /// <param name="entity">The medication.</param>
    /// <returns>The resource.</returns>
    public static MedicationResource ToResourceFromEntity(Medication entity) =>
        new(entity.Id, entity.OlderAdultId, entity.Name, entity.Dosage, entity.Frequency, [..entity.ScheduleTimes],
            entity.Instructions, entity.Active);
}
