using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Query service of the <see cref="DailyReport" /> aggregate.
/// </summary>
public interface IDailyReportQueryService
{
    /// <summary>
    ///     Lists every daily report (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden.</returns>
    Task<Result<IReadOnlyList<DailyReport>, MonitoringQueryError>> Handle(GetAllDailyReportsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the daily reports of an older adult.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden when the requester cannot read the older adult.</returns>
    Task<Result<IReadOnlyList<DailyReport>, MonitoringQueryError>> Handle(GetDailyReportsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a daily report by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The record, NotFound or Forbidden.</returns>
    Task<Result<DailyReport, MonitoringQueryError>> Handle(GetDailyReportByIdQuery query,
        CancellationToken cancellationToken = default);
}
