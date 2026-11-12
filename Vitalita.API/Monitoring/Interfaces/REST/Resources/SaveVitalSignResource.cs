using System.ComponentModel.DataAnnotations;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/vital-signs</c>.
/// </summary>
/// <remarks>
///     <c>unit</c> and <c>id</c> are ignored: the server derives the unit from the type. Ranges: HeartRate 20-250,
///     BloodPressure 50-260 / 30-160 (systolic greater than diastolic), OxygenSaturation 50-100, Temperature 30-45,
///     Glucose 20-600.
/// </remarks>
/// <param name="OlderAdultId">Older adult measured (required).</param>
/// <param name="Type">HeartRate, BloodPressure, OxygenSaturation, Temperature or Glucose (required).</param>
/// <param name="Value">Main value (required; systolic for blood pressure).</param>
/// <param name="SecondaryValue">Diastolic value: required for blood pressure, <c>null</c> for the other types.</param>
/// <param name="MeasuredAt">UTC instant of the measurement (required, not in the future).</param>
public record SaveVitalSignResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? OlderAdultId,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(VitalSignType.HeartRate), nameof(VitalSignType.BloodPressure),
        nameof(VitalSignType.OxygenSaturation), nameof(VitalSignType.Temperature), nameof(VitalSignType.Glucose),
        ErrorMessage = "InvalidVitalSignType")]
    string Type,
    [Required(ErrorMessage = "RequiredField")]
    decimal? Value,
    decimal? SecondaryValue,
    [Required(ErrorMessage = "RequiredField")]
    DateTime? MeasuredAt);
