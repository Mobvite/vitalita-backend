using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Command service of the <see cref="DailyReport" /> aggregate.
/// </summary>
public interface IDailyReportCommandService
{
    /// <summary>
    ///     Records a daily report.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created record or the error.</returns>
    Task<Result<DailyReport, DailyReportCommandError>> Handle(CreateDailyReportCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces a daily report.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated record or the error.</returns>
    Task<Result<DailyReport, DailyReportCommandError>> Handle(UpdateDailyReportCommand command,
        CancellationToken cancellationToken = default);
}
