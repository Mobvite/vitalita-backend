namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Kind of vital sign. Values match the frontend enum.
/// </summary>
public enum VitalSignType
{
    /// <summary>Heart rate, in bpm.</summary>
    HeartRate,

    /// <summary>Blood pressure, in mmHg: systolic in <c>value</c>, diastolic in <c>secondaryValue</c>.</summary>
    BloodPressure,

    /// <summary>Oxygen saturation, in %.</summary>
    OxygenSaturation,

    /// <summary>Body temperature, in °C.</summary>
    Temperature,

    /// <summary>Blood glucose, in mg/dL.</summary>
    Glucose
}
