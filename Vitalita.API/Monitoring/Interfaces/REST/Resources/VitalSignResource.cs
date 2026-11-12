using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Vital sign measurement.
/// </summary>
/// <param name="Id">Measurement identifier.</param>
/// <param name="OlderAdultId">Older adult measured.</param>
/// <param name="Type">HeartRate, BloodPressure, OxygenSaturation, Temperature or Glucose.</param>
/// <param name="Value">Main value (systolic for blood pressure).</param>
/// <param name="SecondaryValue">Diastolic value for blood pressure; <c>null</c> otherwise.</param>
/// <param name="Unit">Unit of the type (bpm, mmHg, %, °C, mg/dL).</param>
/// <param name="MeasuredAt">UTC instant of the measurement.</param>
public record VitalSignResource(
    int Id,
    int OlderAdultId,
    VitalSignType Type,
    decimal Value,
    decimal? SecondaryValue,
    string Unit,
    DateTime MeasuredAt);
