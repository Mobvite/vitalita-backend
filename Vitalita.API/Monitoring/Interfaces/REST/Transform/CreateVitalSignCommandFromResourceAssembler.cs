using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveVitalSignResource" /> into a <see cref="CreateVitalSignCommand" />.
/// </summary>
public static class CreateVitalSignCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateVitalSignCommand ToCommandFromResource(Requester requester, SaveVitalSignResource resource) =>
        new(requester, resource.OlderAdultId!.Value, Enum.Parse<VitalSignType>(resource.Type), resource.Value!.Value,
            resource.SecondaryValue, resource.MeasuredAt!.Value);
}
