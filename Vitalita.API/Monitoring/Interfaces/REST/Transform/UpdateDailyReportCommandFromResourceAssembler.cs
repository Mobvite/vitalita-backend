using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveDailyReportResource" /> into a <see cref="UpdateDailyReportCommand" />.
/// </summary>
public static class UpdateDailyReportCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="id">Record identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateDailyReportCommand ToCommandFromResource(int id, Requester requester,
        SaveDailyReportResource resource) =>
        new(id, requester, resource.OlderAdultId!.Value, resource.ReportDate!.Value, Enum.Parse<Mood>(resource.Mood),
            Enum.Parse<GeneralCondition>(resource.GeneralCondition), resource.Observations);
}
