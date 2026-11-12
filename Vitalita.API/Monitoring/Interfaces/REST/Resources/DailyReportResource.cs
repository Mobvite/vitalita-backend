using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Daily report.
/// </summary>
/// <param name="Id">Report identifier.</param>
/// <param name="OlderAdultId">Older adult of the report.</param>
/// <param name="RecordedByUserId">IAM user that wrote it.</param>
/// <param name="ReportDate">UTC instant of the report.</param>
/// <param name="Mood">VeryGood, Good, Neutral, Bad or VeryBad.</param>
/// <param name="GeneralCondition">Stable, Attention or Critical.</param>
/// <param name="Observations">Observations.</param>
public record DailyReportResource(
    int Id,
    int OlderAdultId,
    int RecordedByUserId,
    DateTime ReportDate,
    Mood Mood,
    GeneralCondition GeneralCondition,
    string Observations);
