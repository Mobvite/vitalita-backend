using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveMedicationResource" /> into the <see cref="MedicationData" /> shared by the create
///     and update commands.
/// </summary>
public static class MedicationDataFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the medication data. A missing <c>active</c> means <c>true</c>.
    /// </summary>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The data.</returns>
    public static MedicationData ToDataFromResource(SaveMedicationResource resource) =>
        new(resource.Name, resource.Dosage, resource.Frequency, resource.ScheduleTimes, resource.Instructions,
            resource.Active ?? true);
}
