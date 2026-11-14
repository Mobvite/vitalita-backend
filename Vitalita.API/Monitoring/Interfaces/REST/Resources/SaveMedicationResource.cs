using System.ComponentModel.DataAnnotations;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/medications</c>. An <c>id</c> in the body is ignored.
/// </summary>
/// <param name="OlderAdultId">Older adult that takes it (required).</param>
/// <param name="Name">Medicine name (required, up to 100 characters).</param>
/// <param name="Dosage">Dosage (required, up to 50 characters).</param>
/// <param name="Frequency">Frequency (up to 100 characters).</param>
/// <param name="ScheduleTimes">1 to 12 times <c>HH:mm</c> (required); stored sorted and without duplicates.</param>
/// <param name="Instructions">Instructions (up to 500 characters).</param>
/// <param name="Active"><c>true</c> by default; <c>false</c> deactivates the medication (there is no delete).</param>
public record SaveMedicationResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? OlderAdultId,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(MonitoringLimits.MedicationNameMaxLength, ErrorMessage = "MedicationNameLength")]
    string Name,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(MonitoringLimits.DosageMaxLength, ErrorMessage = "DosageLength")]
    string Dosage,
    [Required(ErrorMessage = "RequiredField")]
    [MinLength(1, ErrorMessage = "ScheduleTimesLimit")]
    [MaxLength(MonitoringLimits.ScheduleTimesMaxCount, ErrorMessage = "ScheduleTimesLimit")]
    [ItemsRegularExpression(MonitoringLimits.ScheduleTimePattern, ErrorMessage = "ScheduleTimesLimit")]
    IReadOnlyList<string> ScheduleTimes,
    [StringLength(MonitoringLimits.FrequencyMaxLength, ErrorMessage = "FrequencyLength")]
    string? Frequency = null,
    [StringLength(MonitoringLimits.InstructionsMaxLength, ErrorMessage = "InstructionsLength")]
    string? Instructions = null,
    bool? Active = null);
