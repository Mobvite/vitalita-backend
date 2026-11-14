using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Commands;

/// <summary>
///     Command to record a daily report. The author is the requester.
/// </summary>
/// <param name="Requester">The authenticated user (author of the report).</param>
/// <param name="OlderAdultId">Older adult of the report.</param>
/// <param name="ReportDate">UTC instant of the report.</param>
/// <param name="Mood">Mood of the older adult.</param>
/// <param name="GeneralCondition">General condition of the older adult.</param>
/// <param name="Observations">Observations.</param>
public record CreateDailyReportCommand(
    Requester Requester,
    int OlderAdultId,
    DateTime ReportDate,
    Mood Mood,
    GeneralCondition GeneralCondition,
    string Observations);
