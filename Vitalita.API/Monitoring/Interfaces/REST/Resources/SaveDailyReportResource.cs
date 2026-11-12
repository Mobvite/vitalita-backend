using System.ComponentModel.DataAnnotations;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using ConditionValue = Vitalita.API.Monitoring.Domain.Model.ValueObjects.GeneralCondition;
using MoodValue = Vitalita.API.Monitoring.Domain.Model.ValueObjects.Mood;

namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/daily-reports</c>.
/// </summary>
/// <remarks>
///     The frontend also sends <c>id</c> and <c>recordedByUserId</c>; they are ignored (the author comes from the
///     token and is kept on update).
/// </remarks>
/// <param name="OlderAdultId">Older adult of the report (required).</param>
/// <param name="ReportDate">UTC instant of the report (required, not in the future).</param>
/// <param name="Mood">VeryGood, Good, Neutral, Bad or VeryBad (required).</param>
/// <param name="GeneralCondition">Stable, Attention or Critical (required).</param>
/// <param name="Observations">Observations (required, up to 1000 characters).</param>
public record SaveDailyReportResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? OlderAdultId,
    [Required(ErrorMessage = "RequiredField")]
    DateTime? ReportDate,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(MoodValue.VeryGood), nameof(MoodValue.Good), nameof(MoodValue.Neutral), nameof(MoodValue.Bad),
        nameof(MoodValue.VeryBad), ErrorMessage = "InvalidMood")]
    string Mood,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(ConditionValue.Stable), nameof(ConditionValue.Attention),
        nameof(ConditionValue.Critical), ErrorMessage = "InvalidGeneralCondition")]
    string GeneralCondition,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(MonitoringLimits.ObservationsMaxLength, ErrorMessage = "ObservationsLength")]
    string Observations);
