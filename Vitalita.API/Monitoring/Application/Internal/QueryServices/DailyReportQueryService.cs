using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="DailyReport" /> queries.
/// </summary>
/// <remarks>
///     Reads need the caregiver in charge or a family member with an active access (administrators read
///     everything). An older adult the requester cannot read, or that does not exist, is <c>Forbidden</c>.
/// </remarks>
/// <param name="dailyReportRepository">Daily report repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
public class DailyReportQueryService(
    IDailyReportRepository dailyReportRepository,
    OlderAdultAccessPolicy accessPolicy)
    : IDailyReportQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DailyReport>, MonitoringQueryError>> Handle(GetAllDailyReportsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess((await dailyReportRepository.ListAsync(cancellationToken)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<DailyReport>, MonitoringQueryError>> Handle(
        GetDailyReportsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await accessPolicy.CanReadAsync(query.Requester, query.OlderAdultId, cancellationToken))
            return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess(await dailyReportRepository.FindByOlderAdultIdAsync(query.OlderAdultId, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<DailyReport, MonitoringQueryError>> Handle(GetDailyReportByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var record = await dailyReportRepository.FindByIdAsync(query.Id, cancellationToken);
        if (record is null) return new Result<DailyReport, MonitoringQueryError>.Failure(MonitoringQueryError.NotFound);
        return await accessPolicy.CanReadAsync(query.Requester, record.OlderAdultId, cancellationToken)
            ? new Result<DailyReport, MonitoringQueryError>.Success(record)
            : new Result<DailyReport, MonitoringQueryError>.Failure(MonitoringQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<DailyReport>, MonitoringQueryError>
        ListSuccess(IReadOnlyList<DailyReport> records) =>
        new Result<IReadOnlyList<DailyReport>, MonitoringQueryError>.Success(records);

    private static Result<IReadOnlyList<DailyReport>, MonitoringQueryError> ListFailure(MonitoringQueryError error) =>
        new Result<IReadOnlyList<DailyReport>, MonitoringQueryError>.Failure(error);
}
