using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="DailyReport" /> into a <see cref="DailyReportResource" />.
/// </summary>
public static class DailyReportResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource.
    /// </summary>
    /// <param name="entity">The daily report.</param>
    /// <returns>The resource.</returns>
    public static DailyReportResource ToResourceFromEntity(DailyReport entity) =>
        new(entity.Id, entity.OlderAdultId, entity.RecordedByUserId, entity.ReportDate, entity.Mood,
            entity.GeneralCondition, entity.Observations);
}
