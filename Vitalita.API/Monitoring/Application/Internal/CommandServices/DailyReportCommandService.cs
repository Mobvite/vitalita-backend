using Microsoft.EntityFrameworkCore;
using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Monitoring.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="DailyReport" /> commands.
/// </summary>
/// <remarks>Only the caregiver in charge (or an administrator) writes reports; the author comes from the token.</remarks>
/// <param name="dailyReportRepository">Daily report repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
/// <param name="clock">Clock used to reject instants in the future.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class DailyReportCommandService(
    IDailyReportRepository dailyReportRepository,
    OlderAdultAccessPolicy accessPolicy,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<DailyReportCommandService> logger)
    : IDailyReportCommandService
{
    /// <inheritdoc />
    public async Task<Result<DailyReport, DailyReportCommandError>> Handle(CreateDailyReportCommand command,
        CancellationToken cancellationToken = default)
    {
        switch (await accessPolicy.CheckNewRecordAsync(command.Requester, command.OlderAdultId, cancellationToken))
        {
            case NewRecordAccess.Forbidden: return Fail(DailyReportCommandError.Forbidden);
            case NewRecordAccess.OlderAdultNotFound: return Fail(DailyReportCommandError.OlderAdultNotFound);
        }

        DailyReport report;
        try
        {
            report = new DailyReport(command.OlderAdultId, command.Requester.UserId, command.ReportDate, command.Mood,
                command.GeneralCondition, command.Observations, clock.UtcNow);
        }
        catch (InstantInFutureException)
        {
            return Fail(DailyReportCommandError.InstantInFuture);
        }

        try
        {
            await dailyReportRepository.AddAsync(report, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Daily report {ReportId} recorded for older adult {OlderAdultId}", report.Id,
                report.OlderAdultId);
            return new Result<DailyReport, DailyReportCommandError>.Success(report);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while recording a daily report");
            return Fail(DailyReportCommandError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DailyReport, DailyReportCommandError>> Handle(UpdateDailyReportCommand command,
        CancellationToken cancellationToken = default)
    {
        var report = await dailyReportRepository.FindByIdAsync(command.Id, cancellationToken);
        if (report is null) return Fail(DailyReportCommandError.NotFound);
        if (!await accessPolicy.CanWriteAsync(command.Requester, report.OlderAdultId, cancellationToken))
            return Fail(DailyReportCommandError.Forbidden);

        try
        {
            report.Update(command.OlderAdultId, command.ReportDate, command.Mood, command.GeneralCondition,
                command.Observations, clock.UtcNow);
        }
        catch (OlderAdultChangeNotAllowedException)
        {
            return Fail(DailyReportCommandError.OlderAdultChangeNotAllowed);
        }
        catch (InstantInFutureException)
        {
            return Fail(DailyReportCommandError.InstantInFuture);
        }

        try
        {
            dailyReportRepository.Update(report);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<DailyReport, DailyReportCommandError>.Success(report);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating daily report {ReportId}", command.Id);
            return Fail(DailyReportCommandError.UnexpectedError);
        }
    }

    private static Result<DailyReport, DailyReportCommandError> Fail(DailyReportCommandError error) =>
        new Result<DailyReport, DailyReportCommandError>.Failure(error);
}
