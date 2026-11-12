using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Commands;

/// <summary>
///     Command to replace a daily report. The original author is kept.
/// </summary>
/// <param name="Id">Report identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult sent by the client; it must be the current one.</param>
/// <param name="ReportDate">UTC instant of the report.</param>
/// <param name="Mood">Mood of the older adult.</param>
/// <param name="GeneralCondition">General condition of the older adult.</param>
/// <param name="Observations">Observations.</param>
public record UpdateDailyReportCommand(
    int Id,
    Requester Requester,
    int OlderAdultId,
    DateTime ReportDate,
    Mood Mood,
    GeneralCondition GeneralCondition,
    string Observations);
