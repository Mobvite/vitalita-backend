using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveVitalSignResource" /> into a <see cref="UpdateVitalSignCommand" />.
/// </summary>
public static class UpdateVitalSignCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="id">Record identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateVitalSignCommand ToCommandFromResource(int id, Requester requester,
        SaveVitalSignResource resource) =>
        new(id, requester, resource.OlderAdultId!.Value, Enum.Parse<VitalSignType>(resource.Type),
            resource.Value!.Value, resource.SecondaryValue, resource.MeasuredAt!.Value);
}
