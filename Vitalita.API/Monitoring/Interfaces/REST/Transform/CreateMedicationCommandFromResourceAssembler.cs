using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveMedicationResource" /> into a <see cref="CreateMedicationCommand" />.
/// </summary>
public static class CreateMedicationCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateMedicationCommand ToCommandFromResource(Requester requester, SaveMedicationResource resource) =>
        new(requester, resource.OlderAdultId!.Value, MedicationDataFromResourceAssembler.ToDataFromResource(resource));
}
